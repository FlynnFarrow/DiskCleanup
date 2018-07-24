using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Sunburst.Presentation.Controls
{
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    [TemplateVisualState(Name = "Enabled", GroupName = "EnabledStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "EnabledStates")]
    [TemplateVisualState(Name = "Normal", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "Focused", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "MouseOver", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "Pressed", GroupName = "FocusStates")]
    [ContentProperty("Menu")]
    public class MenuButton : ContentControl
    {
        static MenuButton()
        {
            IsEnabledProperty.OverrideMetadata(typeof(MenuButton), new UIPropertyMetadata(OnEnabledChanged));
        }

        public static readonly DependencyProperty MenuProperty =
            DependencyProperty.Register(nameof(Menu), typeof(ContextMenu), typeof(MenuButton), new PropertyMetadata(null, OnMenuChanged));

        public ContextMenu Menu
        {
            get => (ContextMenu)GetValue(MenuProperty);
            set => SetValue(MenuProperty, value);
        }

        private static void OnMenuChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MenuButton button = (MenuButton)sender;
            button.UpdateStates(true);

            if (e.OldValue != null) ((ContextMenu)e.OldValue).Closed -= button.OnMenuClosed;

            if (e.NewValue != null)
            {
                ContextMenu menu = (ContextMenu)e.NewValue;
                menu.PlacementTarget = button;
                menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                menu.Closed += button.OnMenuClosed;
            }
        }

        private static void OnEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MenuButton button = (MenuButton)sender;
            button.UpdateStates(true);
        }

        private void OnMenuClosed(object sender, RoutedEventArgs e)
        {
            UpdateStates(true);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateStates(false);

            ContentPresenter presenter = (ContentPresenter)GetTemplateChild("PART_ContentPresenter");
            AccessKeyManager.AddAccessKeyPressedHandler(presenter, AccessKeyPressed);
        }

        private void UpdateStates(bool useTransitions)
        {
            if (IsEnabled) VisualStateManager.GoToState(this, "Enabled", useTransitions);
            else VisualStateManager.GoToState(this, "Disabled", useTransitions);

            if (Menu != null && Menu.IsOpen) VisualStateManager.GoToState(this, "Pressed", useTransitions);
            else if (IsMouseOver) VisualStateManager.GoToState(this, "MouseOver", useTransitions);
            else if (IsFocused) VisualStateManager.GoToState(this, "Focused", useTransitions);
            else VisualStateManager.GoToState(this, "Normal", useTransitions);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            UpdateStates(true);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            UpdateStates(true);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateStates(true);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateStates(true);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            Focus();
            if (IsEnabled) ToggleMenu();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (IsFocused)
            {
                if (e.Key == Key.Space || e.Key == Key.Enter)
                {
                    ToggleMenu();
                }
            }
        }

        private void AccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
        {
            if (IsEnabled)
            {
                Focus();
                ToggleMenu();
            }
        }

        private void ToggleMenu()
        {
            ContextMenu menu = Menu;
            if (menu == null || menu.Items.Count == 0) return;
            menu.IsOpen = !menu.IsOpen;
        }
    }
}
