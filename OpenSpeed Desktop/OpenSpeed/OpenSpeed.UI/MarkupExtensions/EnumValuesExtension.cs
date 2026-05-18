using System;
using System.Windows.Markup;

namespace OpenSpeed.UI.MarkupExtensions
{
  public class EnumValuesExtension : MarkupExtension
  {
    public required Type EnumType { get; set; }

    override public object ProvideValue(IServiceProvider serviceProvider)
      => Enum.GetValues(EnumType);
  }
}