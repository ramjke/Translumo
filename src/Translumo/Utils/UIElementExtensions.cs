using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SharpDX.XInput;
using Translumo.HotKeys;
using Translumo.Services;

namespace Translumo.Utils
{
    public static class UIElementExtensions
    {
        private static IControllerInputProvider _controllerProvider;

        private static readonly IList<(EventHandler<GamepadKeyPressedEventArgs>, Action<object, GamepadKeyPressedEventArgs>)> _subscribers;

        static UIElementExtensions()
        {
            _subscribers = new List<(EventHandler<GamepadKeyPressedEventArgs>, Action<object, GamepadKeyPressedEventArgs>)>();
        }

        public static void RegisterUIInputController(this ServiceProvider serviceProvider)
        {
            _controllerProvider = serviceProvider.GetService<IControllerInputProvider>();
        }

        public static void SubscribeControllerKeyDown(this UIElement element, Action<object, GamepadKeyPressedEventArgs> handler)
        {
            if (_controllerProvider == null)
            {
                throw new InvalidOperationException("Controller input is not registered");
            }

            var eventHandler = new EventHandler<GamepadKeyPressedEventArgs>(handler);
            _subscribers.Add((eventHandler, handler));

            _controllerProvider.KeyDown += eventHandler;

        }

        public static void UnsubscribeControllerKeyDown(this UIElement element, Action<object, GamepadKeyPressedEventArgs> handler)
        {
            if (_controllerProvider == null)
            {
                throw new InvalidOperationException("Controller input is not registered");
            }

            var subscriber = _subscribers.FirstOrDefault(s => s.Item2 == handler);
            if (!subscriber.Equals(default))
            {
                _controllerProvider.KeyDown -= subscriber.Item1;
                _subscribers.Remove(subscriber);
            }
        }


        public static void SubscribeControllerKeyUp(this UIElement element, Action<object, GamepadKeyPressedEventArgs> handler)
        {
            if (_controllerProvider == null)
            {
                throw new InvalidOperationException("Controller input is not registered");
            }

            var eventHandler = new EventHandler<GamepadKeyPressedEventArgs>(handler);
            _subscribers.Add((eventHandler, handler));

            _controllerProvider.KeyUp += eventHandler;
        }

        public static void UnsubscribeControllerKeyUp(this UIElement element, Action<object, GamepadKeyPressedEventArgs> handler)
        {
            if (_controllerProvider == null)
            {
                throw new InvalidOperationException("Controller input is not registered");
            }

            var subscriber = _subscribers.FirstOrDefault(s => s.Item2 == handler);
            if (!subscriber.Equals(default))
            {
                _controllerProvider.KeyUp -= subscriber.Item1;
                _subscribers.Remove(subscriber);
            }
        }
    }
}
