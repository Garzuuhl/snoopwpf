// (c) Copyright Cory Plotts.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace Snoop
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Windows;
    using System.Text.RegularExpressions;

	public class PropertyFilter
	{
		private string filterString;
        private bool hasFilterString;
		private Regex filterRegex;

        public PropertyFilter(string filterString, bool showDefaults)
		{
			this.FilterString = filterString;
			this.ShowDefaults = showDefaults;
		}

		public string FilterString
		{
			get => this.filterString;
            set
			{
				this.filterString = value;
                this.hasFilterString = string.IsNullOrEmpty(this.filterString) == false;

				try
				{
					this.filterRegex = new Regex(this.filterString, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
				}
				catch
				{
					this.filterRegex = null;
				}
			}
		}

		public bool ShowDefaults { get; set; }

        public PropertyFilterSet SelectedFilterSet { get; set; }

		public bool IsPropertyFilterSet => this.SelectedFilterSet?.Properties != null;

        public bool Show(PropertyInformation property)
		{
			// use a regular expression if we have one and we also have a filter string.
			if (this.filterRegex != null 
                && this.hasFilterString)
			{
				return this.filterRegex.IsMatch(property.DisplayName) 
                       || (property.Property != null 
                            && this.filterRegex.IsMatch(property.Property.PropertyType.Name));
			}
			// else just check for containment if we don't have a regular expression but we do have a filter string.
			else if (this.hasFilterString)
			{
				if (property.DisplayName.ContainsIgnoreCase(this.FilterString))
                {
                    return true;
                }

                if (property.Property != null 
                    && property.Property.PropertyType.Name.ContainsIgnoreCase(this.FilterString))
                {
                    return true;
                }

                return false;
			}
			// else use the filter set if we have one of those.
			else if (this.IsPropertyFilterSet)
			{
				if (this.SelectedFilterSet.IsPropertyInFilter(property.DisplayName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
			// finally, if none of the above applies
			// just check to see if we're not showing properties at their default values
			// and this property is actually set to its default value
			else
			{
				if (this.ShowDefaults == false 
                    && property.ValueSource.BaseValueSource == BaseValueSource.Default)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
		}
	}

	[DebuggerDisplay("{" + nameof(PropertyFilterSet.DisplayName) + "}")]
	[Serializable]
	public class PropertyFilterSet
	{
        public string DisplayName { get; set; }

        public bool IsDefault { get; set; }

        public bool IsEditCommand { get; set; }

		[IgnoreDataMember]
        public bool IsReadOnly { get; set; }

        public string[] Properties { get; set; }

		public bool IsPropertyInFilter(string property)
		{
			foreach (var filterProp in this.Properties)
			{
				if (property.StartsWith(filterProp, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

        public PropertyFilterSet Clone()
        {
            var src = this;
            return new PropertyFilterSet
            {
                DisplayName = src.DisplayName,
                IsDefault = src.IsDefault,
                IsEditCommand = src.IsEditCommand,
				IsReadOnly = src.IsReadOnly,
                Properties = (string[])src.Properties.Clone()
            };
        }
    }
}