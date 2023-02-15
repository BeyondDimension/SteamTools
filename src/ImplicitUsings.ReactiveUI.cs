// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using DynamicData;
global using DynamicData.Binding;

global using System.Reactive.Subjects;
global using System.Reactive.Linq;
global using System.Reactive.Disposables;
global using System.Windows.Input;

global using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
global using Disposable = System.Reactive.Disposables.Disposable;

global using ReactiveUI;
global using ReactiveUI.Fody.Helpers;