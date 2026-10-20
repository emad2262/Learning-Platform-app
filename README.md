# Learning Platform

منصة تعليمية للأكاديميات ومدرّسي المناهج، منظّمة باستخدام Vertical Slice Architecture.

كل عملية عمل لها مجلد داخل الـ feature الخاصة بها، ويجمع الطلب والـ handler
والتحقق والـ endpoint والنتيجة والـ mapping الخاصين بها.

راجع [دليل تنظيم المشروع](docs/vertical-slice-architecture.md) قبل إضافة feature جديدة.

خطة التنفيذ ومعايير القبول والاعتماديات: [Jira backlog](docs/jira-backlog.md).

## التشغيل

من جذر المستودع:

```powershell
dotnet restore Learning-Platform-app.slnx
dotnet build Learning-Platform-app.slnx --no-restore
dotnet run --project Learning-Platform-app
```

ملف تجربة الطلبات: [LearningPlatform.http](Learning-Platform-app/Requests/LearningPlatform.http).
مسار OpenAPI متاح في بيئة Development.

الهيكل الحالي يحتوي على مجلدات جاهزة للتطوير؛ وجود مجلد عملية لا يعني أن تنفيذها
أو مساراتها أو مصادقتها متاحة بالفعل.
