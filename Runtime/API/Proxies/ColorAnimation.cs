//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.10
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


using System;
using System.Runtime.InteropServices;

namespace Noesis
{

public class ColorAnimation : ColorAnimationBase {
  internal new static ColorAnimation CreateProxy(IntPtr cPtr, bool cMemoryOwn) {
    return new ColorAnimation(cPtr, cMemoryOwn);
  }

  internal ColorAnimation(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) {
  }

  internal static HandleRef getCPtr(ColorAnimation obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  protected internal override Color GetCurrentValueCore(Color defaultOriginValue, Color defaultDestinationValue, AnimationClock animationClock) {
    return GetCurrentValueCoreHelper(defaultOriginValue, defaultDestinationValue, animationClock);
  }

  public static DependencyProperty ByProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ColorAnimation_ByProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty FromProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ColorAnimation_FromProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty ToProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ColorAnimation_ToProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty EasingFunctionProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ColorAnimation_EasingFunctionProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public Nullable<Color> From {
    set {
      NullableColor tempvalue = value;
      NoesisGUI_PINVOKE.ColorAnimation_From_set(swigCPtr, ref tempvalue);
    }

    get {
      IntPtr ret = NoesisGUI_PINVOKE.ColorAnimation_From_get(swigCPtr);
      if (ret != IntPtr.Zero) {
        return Marshal.PtrToStructure<NullableColor>(ret);
      }
      else {
        return new Nullable<Color>();
      }
    }

  }

  public Nullable<Color> To {
    set {
      NullableColor tempvalue = value;
      NoesisGUI_PINVOKE.ColorAnimation_To_set(swigCPtr, ref tempvalue);
    }

    get {
      IntPtr ret = NoesisGUI_PINVOKE.ColorAnimation_To_get(swigCPtr);
      if (ret != IntPtr.Zero) {
        return Marshal.PtrToStructure<NullableColor>(ret);
      }
      else {
        return new Nullable<Color>();
      }
    }

  }

  public Nullable<Color> By {
    set {
      NullableColor tempvalue = value;
      NoesisGUI_PINVOKE.ColorAnimation_By_set(swigCPtr, ref tempvalue);
    }

    get {
      IntPtr ret = NoesisGUI_PINVOKE.ColorAnimation_By_get(swigCPtr);
      if (ret != IntPtr.Zero) {
        return Marshal.PtrToStructure<NullableColor>(ret);
      }
      else {
        return new Nullable<Color>();
      }
    }

  }

  public EasingFunctionBase EasingFunction {
    set {
      NoesisGUI_PINVOKE.ColorAnimation_EasingFunction_set(swigCPtr, EasingFunctionBase.getCPtr(value));
    } 
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ColorAnimation_EasingFunction_get(swigCPtr);
      return (EasingFunctionBase)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  private Color GetCurrentValueCoreHelper(Color src, Color dst, AnimationClock clock) {
    IntPtr ret = NoesisGUI_PINVOKE.ColorAnimation_GetCurrentValueCoreHelper(swigCPtr, ref src, ref dst, AnimationClock.getCPtr(clock));
    if (ret != IntPtr.Zero) {
      return Marshal.PtrToStructure<Color>(ret);
    }
    else {
      return new Color();
    }
  }

  public ColorAnimation() {
  }

  protected override IntPtr CreateCPtr(Type type, out bool registerExtend) {
    if (type == typeof(ColorAnimation)) {
      registerExtend = false;
      return NoesisGUI_PINVOKE.new_ColorAnimation();
    }
    else {
      return base.CreateExtendCPtr(type, out registerExtend);
    }
  }

  internal new static IntPtr Extend(string typeName) {
    return NoesisGUI_PINVOKE.Extend_ColorAnimation(Marshal.StringToHGlobalAnsi(typeName));
  }
}

}
