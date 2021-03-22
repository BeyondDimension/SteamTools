# SmsSender 统一短信发送服务
适用于 **ASP.NET Core 5.0** 的统一的短信服务接口供业务层调用，~~可多个提供商同时使用~~，通过替换实现类的方式无缝切换提供商  
~~可以稍微修改兼容 ```.NET Framework 4.6.1 & ASP.NET 4.x+```，但没必要~~
- 目前支持的提供商
	- [阿里云](https://help.aliyun.com/product/44282.html)
	- [网易云信](https://yunxin.163.com/sms)
	- [世纪互联蓝云](https://bccs.21vbluecloud.com/intro/sms)
