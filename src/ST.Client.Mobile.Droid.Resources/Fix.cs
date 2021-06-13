// 方案一：❌
// 使用 AndroidX DataBinding 
// https://developer.android.google.cn/jetpack/androidx/releases/databinding?hl=zh-cn
// 在 Android Studio 中将资源放入 lib 模块中，生成 aar 包进行绑定
// 缺点：
// Java 泛型擦除导致 继承接口 IViewBinding 类型不正确，如下手动修复，很麻烦
// 生成类的实现每一个控件都进行了 findViewById，即使不用这个控件也会 find
// 可使用 tools:viewBindingIgnore="true" 忽略绑定
// 对 include 支持不太好，要么设置id，生成字段变成二级，要么通过bind多个绑定类
// 不能通过构造函数创建，使用泛型统一创建对象需要反射方法
// 
// 方案二：✔
// 使用 Xamarin.Android Layout CodeBehind 
// https://github.com/xamarin/xamarin-android/blob/main/Documentation/guides/LayoutCodeBehind.md
// 将 资源通过 link 引入项目，通过 ST.Tools.AndroidResourceLink 工具生成项
// 使用 LinkBase 与通配符* 有问题，只能单个文件一个项声明
// 缺点：
// AndroidX 控件绑定托管类型，大小写转换有问题，需要手动修复
// https://github.com/xamarin/xamarin-android/issues/4507
// xmlns:xamarin="http://schemas.xamarin.com/android/xamarin/tools"
// xamarin:managedType="AndroidX.Navigation.Fragment.NavHostFragment"
// xamarin:managedType="Google.Android.Material.BottomNavigation.BottomNavigationView"
// xamarin:managedType="AndroidX.AppCompat.Widget.AppCompatTextView"
// xamarin:managedType="AndroidX.RecyclerView.Widget.RecyclerView"
// xamarin:managedType="AndroidX.AppCompat.Widget.AppCompatImageView"
// xamarin:managedType="AndroidX.AppCompat.Widget.AppCompatEditText"
// xamarin:managedType="Google.Android.Material.Button.MaterialButton"
// xamarin:managedType="Google.Android.Material.ProgressIndicator.CircularProgressIndicator"
// xamarin:managedType="AndroidX.AppCompat.Widget.Toolbar"

using Android.Views;
using AndroidX.ViewBinding;

namespace System.Application.UI.DataBindings
{
    partial class FragmentLoginAndRegisterByFastBinding
    {
        View IViewBinding.Root => Root;
    }
}
