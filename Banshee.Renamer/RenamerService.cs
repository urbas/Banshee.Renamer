// 
// RenamerService.cs
// 
// Author:
//   Matej Urbas <matej.urbas@gmail.com>
// 
// Copyright 2012 matej
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using Banshee.ServiceStack;
using Banshee.Gui;
using Gtk;
using Mono.Unix;

namespace Banshee.Renamer
{
    public class RenamerService : IExtensionService
    {
        #region Fields
        public ActionGroup menuActions;
        public uint menuActionsUiId;
        #endregion

        #region Constants and Static Fields
        public const string Name = "Renamer";
        #endregion

        #region Constructors
        public RenamerService ()
        {
        }
        #endregion

        #region Static Private Helper Properties
        private static InterfaceActionService BansheeActionService {
            get {
                return ServiceManager.Get<InterfaceActionService> ();
            }
        }
        #endregion

        #region Action Handlers
        /// <summary>
        /// This method is invoked when the user clicks on the `Tools -> Rename...`
        /// menu item.
        /// </summary>
        /// <param name='source'>
        /// The source of the click event (the menu item).
        /// </param>
        /// <param name='args'>
        /// Event arguments...
        /// </param>
        protected virtual void OnOpenMassRenamerAction(object source, EventArgs args)
        {
            Console.WriteLine("Currently registered services:");
            foreach (var s in ServiceManager.RegisteredServices) {
                Console.WriteLine(s.ServiceName);
            }
        }
        #endregion

        #region Interface Initialisation and Disposal
        /// <summary>
        /// Initialises the interface of Banshee. It inserts the `rename` menu item
        /// in the `Tools` menu.
        /// </summary>
        private void InitializeUi ()
        {
            // Add the `Rename` menu item to the `Tools` menu.
            menuActions = new ActionGroup ("Renamer");

            menuActions.Add (new ActionEntry [] {
                new ActionEntry ("OpenMassRenamerAction", null,
                    Catalog.GetString ("_Rename..."), null,
                    Catalog.GetString ("Rename selected files."), OnOpenMassRenamerAction)
            });

            BansheeActionService.UIManager.InsertActionGroup (menuActions, 0);
            menuActionsUiId = BansheeActionService.UIManager.AddUiFromResource ("RenamerActionsUI.xml");
        }

        /// <summary>
        /// Disposes of whatever the `InitializeUi` method did.
        /// </summary>
        private void DisposeUi ()
        {
            BansheeActionService.UIManager.RemoveUi (menuActionsUiId);
            BansheeActionService.UIManager.RemoveActionGroup (menuActions);
        }
        #endregion

        #region IExtensionService Implementation
        public string ServiceName {
            get {
                return Name;
            }
        }

        void IExtensionService.Initialize ()
        {
            InitializeUi ();
        }

        void IDisposable.Dispose ()
        {
            DisposeUi ();
        }
        #endregion
    }
}

