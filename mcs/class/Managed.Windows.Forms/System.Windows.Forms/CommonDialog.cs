// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// NOT COMPLETE

namespace System.Windows.Forms {
	public abstract class CommonDialog : System.ComponentModel.Component {
		#region DialogForm
		internal class DialogForm : Form {
			#region FormParentWindow Override
			internal new class FormParentWindow : Form.FormParentWindow {
				internal FormParentWindow(Form owner) : base(owner) {
				}

				protected override CreateParams CreateParams {
					get {
						CreateParams	cp;

						if (owner != null) {
							owner.ControlBox = true;
							owner.MinimizeBox = false;
							owner.MaximizeBox = false;
						}

						cp = base.CreateParams;

						// FIXME - I need to rethink this a bit, we're loosing any cp.Style set in base; might not matter though
						cp.Style = (int)(WindowStyles.WS_POPUP | WindowStyles.WS_CAPTION | WindowStyles.WS_SYSMENU);
						return cp;
					}
				}

			}
			#endregion

			#region DialogForm Local Variables
			protected CommonDialog	owner;
			#endregion DialogForm Local Variables

			#region DialogForm Constructors
			internal DialogForm(CommonDialog owner) {
				this.owner = owner;
			}
			#endregion DialogForm Constructors

			#region Protected Instance Properties
			protected override CreateParams CreateParams {
				get {
					if (this.form_parent_window == null) {
						form_parent_window = new FormParentWindow(this);
					}

					return base.CreateParams;
				}
			}

			protected override void WndProc(ref Message m) {
				base.WndProc (ref m);
			}

			#endregion	// Protected Instance Properties

			#region Internal Methods
			internal DialogResult RunDialog () {
				this.StartPosition = FormStartPosition.CenterScreen;

				owner.InitFormsSize (this);

				this.ShowDialog ();

				return this.DialogResult;

			}
			#endregion Internal Methods
		}
		#endregion DialogForm

		#region Local Variables
		internal DialogForm	form;
		#endregion Local Variables

		#region Public Constructors
		public CommonDialog() {
			form = new DialogForm(this);
		}
		#endregion Public Constructors

		#region Internal Methods
		internal virtual void InitFormsSize(Form form) {
			// Override this to set a default size for the form
			form.Width = 200;
			form.Height = 200;
		}
		#endregion Internal Methods
	
		#region Public Instance Methods
		public abstract void Reset();

		public DialogResult ShowDialog() {
			return ShowDialog(null);
		}

		public DialogResult ShowDialog(IWin32Window ownerWin32) {
			Control		owner = null;

			if (ownerWin32 != null) {
				owner = Control.FromHandle(ownerWin32.Handle);
			}

			RunDialog(form.Handle);

			if (form.Visible) {
				throw new InvalidOperationException("Already visible forms cannot be displayed as a modal dialog. Set the Visible property to 'false' prior to calling Form.ShowDialog.");
			}

			if (!form.IsHandleCreated) {
				form.CreateControl();
			}

			form.form_parent_window.Parent = owner;

			XplatUI.SetModal(form.form_parent_window.Handle, true);

			form.Show();

			form.is_modal = true;
			Application.ModalRun(form);
			form.is_modal = false;
			form.Hide();

			XplatUI.SetModal(form.form_parent_window.Handle, false);

			return form.DialogResult;
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected virtual IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam) {
			return IntPtr.Zero;
		}

		protected virtual void OnHelpRequest(EventArgs e) {
			if (HelpRequest != null) {
				HelpRequest(this, e);
			}
		}

		protected virtual IntPtr OwnerWndProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam) {
			return IntPtr.Zero;
		}

		protected abstract bool RunDialog(IntPtr hwndOwner);
		#endregion	// Protected Instance Methods

		#region Events
		public event EventHandler HelpRequest;
		#endregion	// Events
	}
}
