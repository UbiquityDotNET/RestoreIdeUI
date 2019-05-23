using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;

namespace RestoreIdeUI
{
    [PackageRegistration( UseManagedResourcesOnly = true, AllowsBackgroundLoading = true )]
    [ProvideAutoLoad( UIContextGuids80.EmptySolution, PackageAutoLoadFlags.BackgroundLoad )]
    [ProvideAutoLoad( UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad )]
    [ProvideAutoLoad( UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad )]
    [ProvideOptionPage( typeof( RestoreIdeUIOptionsPage ), "Environment", "Restore IDE", 0, 0, true )]
    [Guid( PackageGuidString )]
    public sealed class RestoreIdeUIPackage
        : AsyncPackage
    {
        public const string PackageGuidString = "4fecf9cd-6a0d-406c-9502-eb3952b50132";

        public RestoreIdeUIPackage( )
        {
            Options = new Lazy<RestoreIdeUIOptionsPage>( ( ) => ( RestoreIdeUIOptionsPage )GetDialogPage( typeof( RestoreIdeUIOptionsPage ) ) );
            LazyCommandFilter = new Lazy<CommandFilter>( ( ) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread( );
                return new CommandFilter( ( IOleCommandTarget )base.GetService( typeof( IOleCommandTarget ) ) );
            } );
        }

        protected override async Task InitializeAsync( CancellationToken cancellationToken, IProgress<ServiceProgressData> progress )
        {
            // switch to UI thread to update the main UI
            await JoinableTaskFactory.SwitchToMainThreadAsync( cancellationToken );

            // hook up command filter to handle enabling the news feed for the StartPage
            var priorityReg = ( IVsRegisterPriorityCommandTarget ) await GetServiceAsync( typeof( SVsRegisterPriorityCommandTarget ) );
            int? hr = priorityReg?.RegisterPriorityCommandTarget( 0, this, out _ );
            if( hr.HasValue && hr.Value != VSConstants.S_OK )
            {
                throw new COMException( "Error registering priority command target", ( int )hr );
            }

            // hook into notification that UI context changed so that start page can be shown
            NoSolutionContext.WhenActivated( ShowStartPage );
            NoSolutionContext.UIContextChanged += NoSolutionContext_UIContextChanged;

            // get and apply the current settings dynamically as hiding search is not supported directly by VS
            Options.Value.LoadSettingsFromStorage( );
            Options.Value.ApplySettings( );
        }

        private void NoSolutionContext_UIContextChanged( object sender, UIContextChangedEventArgs e )
        {
            if( e.Activated )
            {
                ShowStartPage( );
            }
        }

        protected override object GetService( Type service )
        {
            return service == typeof( IOleCommandTarget ) ? LazyCommandFilter.Value : base.GetService( service );
        }

        private void ShowStartPage( )
        {
            if( Options.Value.ShowStartPage )
            {
                var dte = GetService( typeof( SDTE ) ) as EnvDTE80.DTE2;
                dte?.ExecuteCommand( "File.StartPage" );
            }
        }

        private readonly Lazy<CommandFilter> LazyCommandFilter;
        private readonly Lazy<RestoreIdeUIOptionsPage> Options;
        private readonly UIContext NoSolutionContext = UIContext.FromUIContextGuid( new Guid( UIContextGuids80.NoSolution ) );
    }
}
