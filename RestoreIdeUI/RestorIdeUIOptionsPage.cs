using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;

namespace RestoreIdeUI
{
    class RestoreIdeUIOptionsPage
        : DialogPage
    {
        [Category( "Title Bar" )]
        [DisplayName( "Disable VS Search" )]
        [Description( "Disables the VS Search box in the title bar" )]
        public bool DisableVsSearch { get; set; }

        [Category( "Projects" )]
        [DisplayName( "Disable new project dialog" )]
        [Description( "Disables the new \"Modern\" project dialog" )]
        [SharedSettings( "WindowManagement.Options.DisableNewProjectCreationExperience", false )]
        public bool DisableNewProjectDialogExperience { get; set; }

        [Category( "Startup" )]
        [DisplayName( "Show Start Page" )]
        [Description( "Determines if the Start Page is shown when no solution is loaded" )]
        public bool ShowStartPage { get; set; }

        [Category( "Startup" )]
        [DisplayName( "Download news on Start Page" )]
        [Description( "Determines if the Start Page downloads news content" )]
        public bool StartPageDownloadContent { get; set; }

        protected override void OnApply( PageApplyEventArgs e )
        {
            base.OnApply( e );
            if( e.ApplyBehavior == ApplyKind.Apply )
            {
                ApplySettings( );
            }
        }

        internal void ApplySettings()
        {
            // Find and apply visibility of the search box in the title bar
            // This is a dynamic action as the VS UI element has no direct support for hiding the search box anymore [sigh...]
            var searchPresenter = FindChildElement( Application.Current.MainWindow, e => e.GetType( ).Name == "PackageSearchControlPresenter" );
            if( searchPresenter != null )
            {
                searchPresenter.Visibility = DisableVsSearch ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        internal static FrameworkElement FindChildElement( Visual v, Predicate<FrameworkElement> condition )
        {
            if( v == null )
            {
                return null;
            }

            for( int i = 0; i < VisualTreeHelper.GetChildrenCount( v ); ++i )
            {
                var child = VisualTreeHelper.GetChild( v, i ) as Visual;

                if( child is FrameworkElement e && condition( e ) )
                {
                    return e;
                }

                var result = FindChildElement( child, condition );
                if( result != null )
                {
                    return result;
                }
            }

            return null;
        }
    }
}
