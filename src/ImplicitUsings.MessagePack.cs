// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using MessagePack;
global using MessagePack.Formatters;
global using MPConstructor = MessagePack.SerializationConstructorAttribute;
global using MPIgnore = MessagePack.IgnoreMemberAttribute;
global using MPKey = MessagePack.KeyAttribute;
global using MPObj = MessagePack.MessagePackObjectAttribute;
global using MPUnion = MessagePack.UnionAttribute;

global using MemoryPack;
global using MemoryPack.Formatters;
global using MP2Constructor = MemoryPack.MemoryPackConstructorAttribute;
global using MP2Ignore = MemoryPack.MemoryPackIgnoreAttribute;
global using MP2Key = MemoryPack.MemoryPackOrderAttribute;
global using MP2Obj = MemoryPack.MemoryPackableAttribute;
global using MP2Union = MemoryPack.MemoryPackUnionAttribute;