# SqlSugar.InterSystemCore
基于SqlSugar扩展实现IRIS数据库的O/RM，目前支持DBFrist,CodeFrist和常用的增删改查


如果使用过程中出现sql拼接错误或报错，需要下载Git上的完整代码后进进行编译使用哦~

开发进度尚不明确，开发速度随缘，暂无明确开发计划


使用方式：
1.Nuget下载安装项目，安装SqlSugar
2.将InterSystem的IRIS动态库链接添加到项目中
3.实例创建代码如下所示


    //设置调用的扩展
    InstanceFactory.CustomDbName = "InterSystem";
    InstanceFactory.CustomDllName = "SqlSugar.InterSystemCore";
    InstanceFactory.CustomNamespace = "SqlSugar.InterSystemCore";
    
    //设置数据库的链接和调用的方式
    SqlSugarScope db = new SqlSugarScope(new ConnectionConfig()
    {
    ConnectionString = "Server=127.0.0.1;Port=1972; Namespace=MyNameSpacd; Password=sys; User ID =_system;",
    DbType = DbType.Custom,//设置为自定义
    InitKeyType = InitKeyType.Attribute,
    MoreSettings = new ConnMoreSettings() { DisableNvarchar = false }
    });



