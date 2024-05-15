using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace tar.BlazorInput.Models {
  public class INPC: INotifyPropertyChanged {
    #region --- events ----------------------------------------------------------------------------
    public event PropertyChangedEventHandler? PropertyChanged;
    #endregion

    #region --- on property changed ---------------------------------------------------------------
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    #endregion
    #region --- set field -------------------------------------------------------------------------
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
      if (EqualityComparer<T>.Default.Equals(field, value)) {
        return false;
      }

      field = value;
      OnPropertyChanged(propertyName);
      
      return true;
    }
    #endregion
  }
}