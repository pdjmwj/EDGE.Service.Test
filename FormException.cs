using System;
using System.Windows.Forms;

namespace EDGE.Service.Test
{
	public partial class FormException : Form  
    {
        #region | Constructors |

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="exsException">Exception that shall be used to be displayed to the application user.</param>
        public FormException(System.Exception exsException, string routineName)
        {
            InitializeComponent();

            //Custom Init
            this._Exception = exsException;
            this._Routine = routineName;
            this.Init();
        }
        #endregion

        #region | Local Members |

        private System.Exception _Exception = null;
        private string _Routine = null;

        #endregion 

        #region | Properties |

        /// <summary>
        /// Gets the exception that shall be used to display.
        /// </summary>
        public Exception ExsException
        {
            get
            {
                return this._Exception;
            }
        }

        /// <summary>
        /// Gets the routine name that was provided to the form.
        /// </summary>
        public string Routine
        {
            get
            {
                return this._Routine;
            }
        }

        #endregion 

		#region | Events |

		private void buttonClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		#endregion

        #region | Methods |

        /// <summary>
        /// Custom, developer defined, initialization method.
        /// </summary>
        private void Init()
        {
            if (this.ExsException != null)
            {
                this.labelExceptionDateTime.Text = System.DateTime.Now.ToString();
                this.labelExceptionSource.Text = this.ExsException.Source != null ? this.ExsException.Source.Trim() : String.Empty;
                this.textboxExceptionMessage.Text = this.ExsException.Message != null ? this.ExsException.Message.Trim() : String.Empty;
            }

            this.labelRoutine.Text = this.Routine != null ? this.Routine.Trim() : String.Empty;
        }

        #endregion
    }
}