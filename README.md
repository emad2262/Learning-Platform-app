# Learning Platform

منصة تعليمية للأكاديميات ومدرّسي المناهج، منظّمة باستخدام Vertical Slice Architecture.

ملفات العمليات مجمعة داخل الـ feature. الأكاديميات مقسمة إلى Commands وValidation
وOrchestrators وDTOs وViewModels وQueries وControllers.

راجع [دليل تنظيم المشروع](docs/vertical-slice-architecture.md) قبل إضافة feature جديدة.

خطة التنفيذ ومعايير القبول والاعتماديات: [Jira backlog](docs/jira-backlog.md).

تجهيز SQL Server وأول migration والاختبارات: [قاعدة البيانات](docs/database-setup.md).

## التشغيل

من جذر المستودع:

```powershell
dotnet tool restore
dotnet restore Learning-Platform-app.slnx
dotnet build Learning-Platform-app.slnx --no-restore
dotnet ef database update --project Learning-Platform-app -- --environment Development
dotnet run --project Learning-Platform-app
```

مسار OpenAPI متاح في بيئة Development.

الموديل الأساسي للأكاديمية ومالكها وبروفايلات الطلاب والمدرّسين وIdentity موجود.
العضويات والأدوار داخل الأكاديمية مؤجلة، والعلاقات الحالية مباشرة مع الحساب والأكاديمية. مجلدات
الـ features جاهزة للتطوير؛ وجودها لا يعني أن عملياتها أو مساراتها منفذة.
