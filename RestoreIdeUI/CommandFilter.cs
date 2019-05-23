// -----------------------------------------------------------------------
// <copyright file="CommandFilter.cs" company="Ubiquity.NET Contributors">
// Copyright (c) Ubiquity.NET Contributors. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

using Task = System.Threading.Tasks.Task;

namespace RestoreIdeUI
{
    // Command filtering to catch when the start page is loaded
    // and then modify the page to trigger a change in the properties
    // that controls whether the RSS feed download is enabled.
    internal class CommandFilter
        : IOleCommandTarget
    {
        public CommandFilter( IOleCommandTarget innerTarget )
        {
            ThreadHelper.ThrowIfNotOnUIThread( );
            InnerTarget = innerTarget;
        }

        public int QueryStatus( ref Guid pguidCmdGroup, uint cCmds, OLECMD[ ] prgCmds, IntPtr pCmdText )
        {
            ThreadHelper.ThrowIfNotOnUIThread( );
            return InnerTarget.QueryStatus( pguidCmdGroup, cCmds, prgCmds, pCmdText );
        }

        public int Exec( ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut )
        {
            ThreadHelper.ThrowIfNotOnUIThread( );
            int retVal = InnerTarget.Exec( pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut );
            if( ( nCmdID == ( uint )VSConstants.VSStd2KCmdID.StartPage ) && ( pguidCmdGroup == VSConstants.VSStd2K ) )
            {
                // post work onto UI thread after a 1 second delay. This ensures the page is
                // actually created before attempting to modify any of the page's content.
                // JoinableTaskFactory is the WRONG thing to do here, as it blocks everything
                // from the UI thread until the entire task completes and this is already on
                // the UI thread.
                Task.Delay( 1000 ).ConfigureAwait( true ).GetAwaiter( ).OnCompleted( EnableNewsFeed );
            }

            return retVal;
        }

        [SuppressMessage( "Design", "CA1031:Do not catch general exception types", Justification = "Too many undocumented exceptions could occur and this is a best effort action" )]
        [SuppressMessage( "Potential Code Quality Issues", "RECS0022:A catch clause that catches System.Exception and has an empty body", Justification = "ditto" )]
        private static void EnableNewsFeed( )
        {
            // This is where things get UGLY/Fragile
            try
            {
                // In VS2019 the DownloadEnabled property binding for the data source was removed so,
                // this needs to add enough support back to allow the XAML UI to correctly see
                // the property from the source to trigger downloading contents of the feed.
                var feedControl = RestoreIdeUIOptionsPage.FindChildElement( Application.Current.MainWindow, e => e.GetType( ).Name == "RssFeedControl" );

                // data types, property names etc.. of internal types was determined by attaching the debugger
                // and the Live XAML viewer to see what the types are. Obviously this is fragile, but we have
                // little choice when the VS team has gone to great lengths to make it hard to keep perfectly good working UI.

                // DataContext for the RssFeedControl is an StartPageDataSource wrapped inside of an internal DataSource type
                var ds = ( DataSource )feedControl.DataContext;

                // StartPageDataSource has an "Rss" property that contains an instance of the internal RssFeedDataSource type
                var rss = ( DataSource )ds.GetValue( "Rss" );
                var field = typeof( DataSource ).GetField( "innerSource", BindingFlags.Instance | BindingFlags.NonPublic );

                // RssFeedDataSource, while internal, derives from the public UIDataSource type
                var rssd = ( UIDataSource )field.GetValue( rss );
                var rssdType = rssd.GetType( );

                // RssFeed property resolves to dead link for Community editions, so reset it to
                // the same link as other SKUs
                var feedProp = rssdType.GetRuntimeProperty( "RssFeed" );
                string currentFeed = ( string )feedProp.GetValue( rssd );
                string newFeed = currentFeed.Replace( CommunityLinkId, CorrectedLinkId );
                feedProp.SetValue( rssd, newFeed );

                // need to set the _announcementsFeed as it backs the Get only AnnouncementsFeed property
                var announcmentsFeedField = rssdType.GetField( "_announcementsFeed", BindingFlags.NonPublic | BindingFlags.Instance );
                announcmentsFeedField.SetValue( rssd, newFeed );

                // RssFeedDataSourceBase declares a virtual "OnPropertyChanged" that updates the rest,
                // so force a change of the DownloadEnabled property
                var method = rssdType.GetMethod( "OnPropertyChanged", BindingFlags.Instance | BindingFlags.Public );
                if( method != null )
                {
                    object[ ] parameters = { /*IVsUIDataSource ds*/null, "DownloadEnabled", /*IVsUIObject pVarOld*/null, BuiltInPropertyValue.FromBool( true ) };
                    /*void*/
                    _ = method.Invoke( rssd, parameters );
                }
            }
            catch
            {
                /* NOP */
            }
        }

        private const string CorrectedLinkId = "661023";
        private const string CommunityLinkId = "$(StartPageRssFeedFwlinkId)";
        private readonly IOleCommandTarget InnerTarget;
    }
}
