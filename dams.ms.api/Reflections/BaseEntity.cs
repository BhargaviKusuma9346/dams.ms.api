using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;


namespace Dams.ms.auth.Reflections
{
    public sealed class ValidateEventArgs : EventArgs
    {
        private String _propertyName;
        private Boolean? _isValid;
        private String _message;
        // Summary:
        //     Initializes a new instance of the System.ComponentModel.DataErrorsChangedEventArgs
        //     class.
        //
        // Parameters:
        //   propertyName:
        //     The name of the property for which the errors changed, or null or System.String.Empty
        //     if the errors affect multiple properties.
        public ValidateEventArgs(string propertyName)
        {
            this._propertyName = propertyName;
        }

        // Summary:
        //     Gets the name of the property for which the errors changed, or null or System.String.Empty
        //     if the errors affect multiple properties.
        //
        // Returns:
        //     The name of the affected property, or null or System.String.Empty if the
        //     errors affect multiple properties.
        public string PropertyName
        {
            get
            {
                return _propertyName;
            }
            private set
            {
                _propertyName = value;
            }
        }
        /// <summary>
        /// Message
        /// </summary>
        public String Message
        {
            get { return _message; }
            set { _message = value; }
        }
        /// <summary>
        /// Is validatation valid or not?
        /// </summary>
        public Boolean? IsValid
        {
            get
            {
                return _isValid;
            }
            set
            {
                _isValid = value;
            }
        }
    }
	public abstract class BaseEntity : INotifyPropertyChanged
	{
		#region Private Variables


        [NonSerialized]
        private Boolean _isValidationValid;
        [NonSerialized]
        private String _validationTrackingId;
		#endregion
        
		#region Public Methods


		protected void OnPropertyChanged(string property)
		{
			OnPropertyChanged(property, true);
		}

		private void OnPropertyChanged(string property, bool setDirtyFlag)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(property));
			}		
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#endregion


        /// <summary>Is the validation of this object up to date?</summary>
        /// <param name="validationTrackingId">Tracking ID for validation so we don't check objects twice!</param>
        private Boolean ValidateAll(String validationTrackingId)
        {
            //Check to see if we have already been here, if so then respond with our last check value.
            if (this._validationTrackingId == validationTrackingId)
            {
                return true;//we already reported this as errored out//this._isValidationValid;
            }
            this._validationTrackingId = validationTrackingId;
            _isValidationValid = true;
            PropertyInfo[] properties = GetProperties(this);
            foreach (PropertyInfo property in properties)
            {
                //ValidateProperty(property, validationTrackingId);
            }
            return this._isValidationValid;
        }

        #region Check Requirement Needs
        //private void CheckRequirementChangesNeeded(String properyNameChanged)
        //{
        //    String validationTracking = System.Guid.NewGuid().ToString();
        //    foreach (PropertyInfo property in this.GetType().GetProperties())
        //    {
        //        foreach (Attribute attribute in Attribute.GetCustomAttributes(property, typeof( AutoCRM.Components.Validation.WhenFunctionDependency)))
        //        {
        //             AutoCRM.Components.Validation.WhenFunctionDependency when = attribute as  AutoCRM.Components.Validation.WhenFunctionDependency;
        //            if (when != null && when.Name == properyNameChanged)
        //            {
        //                //ValidateProperty(property, validationTracking);
        //                OnPropertyChanged(property.Name);
        //            }
        //        }
        //         AutoCRM.Components.Validation.Validator validator = Attribute.GetCustomAttribute(property, typeof( AutoCRM.Components.Validation.Validator)) as  AutoCRM.Components.Validation.Validator;
        //        if (validator != null && validator.RequiredWhenFunction != null)
        //        {
        //            MethodInfo method = this.GetType().GetMethod(validator.RequiredWhenFunction);
        //            if (method != null)
        //            {
        //                foreach (Attribute attribute in Attribute.GetCustomAttributes(method, typeof( AutoCRM.Components.Validation.WhenFunctionDependency)))
        //                {
        //                    AutoCRM.Components.Validation.WhenFunctionDependency when = attribute as  AutoCRM.Components.Validation.WhenFunctionDependency;
        //                    if (when != null && when.Name == properyNameChanged)
        //                    {
        //                       // ValidateProperty(property, validationTracking);
        //                        OnPropertyChanged(property.Name);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        //private Boolean CheckIfRequirementIsOverridden(Validator validator, Boolean defaultIsRequired)
        //{
        //    if (validator.RequiredWhenFunction != null)
        //    {
        //        MethodInfo method = this.GetType().GetMethod(validator.RequiredWhenFunction);
        //        if (method != null)
        //        {
        //            return (((Boolean)method.Invoke(this, null)));
        //        }
        //    }
        //    return defaultIsRequired;
        //}
        #endregion

        #region Get Properties
        /// <summary>
        /// Get Properties
        /// </summary>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties(Object entity)
        {
            PropertyInfo[] properties = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return properties.Where(p => p.Name != "Item").ToArray<PropertyInfo>();
        }
        /// <summary>
        /// Get Properties
        /// </summary>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties(Type type)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return properties.Where(p => p.Name != "Item").ToArray<PropertyInfo>();
        }
        #endregion
    }
}
