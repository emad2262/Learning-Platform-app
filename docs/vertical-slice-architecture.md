# تنظيم المشروع باستخدام Vertical Slice

المشروع تطبيق واحد. تنظيم العمليات حسب الـ feature. داخل Academies التقسيم حسب
المسؤولية (Commands وValidation وOrchestrators وDTOs وViewModels وQueries
وControllers)، وباقي الـ features تحتفظ حاليًا بتقسيمها حسب العملية.
المجلدات المجهزة هي أماكن لإضافة التنفيذ؛ لا تحتوي حاليًا على handlers أو خدمات
أو entities من النسخة السابقة.

## الهيكل

```text
Learning-Platform-app/
├── Features/
│   ├── Accounts/
│   │   ├── Register/
│   │   └── Login/
│   ├── Academies/
│   │   ├── Commands/
│   │   ├── Validation/
│   │   ├── Orchestrators/
│   │   ├── DTOs/
│   │   ├── ViewModels/
│   │   ├── Queries/
│   │   └── Controllers/
│   ├── Memberships/JoinAcademy/
│   ├── Courses/
│   │   ├── CreateCourse/
│   │   └── GetCourses/
│   ├── Lessons/CreateLesson/
│   ├── Enrollments/
│   │   ├── EnrollInCourse/
│   │   └── GetMyCourses/
│   ├── Exams/
│   │   ├── CreateExam/
│   │   ├── PublishExam/
│   │   ├── StartAttempt/
│   │   ├── SubmitAttempt/
│   │   ├── GetAttemptResult/
│   │   └── Orchestrators/
│   ├── Subscriptions/SubscribeToPlan/
│   └── CoursePurchases/
│       ├── CreatePurchase/
│       ├── ConfirmPayment/
│       └── Orchestrators/
├── Domain/
│   ├── Entities/
│   └── Enums/
├── Common/Abstractions/
│   ├── Persistence/
│   ├── Transactions/
│   └── Payments/
├── Infrastructure/
│   ├── Persistence/
│   │   ├── Configurations/
│   │   ├── Repositories/
│   │   └── Seed/
│   ├── Transactions/
│   ├── Payments/
│   └── Storage/
├── Requests/LearningPlatform.http
├── Properties/
├── Program.cs
└── appsettings.json
```

## ملفات العملية الواحدة

مثال عند تنفيذ إنشاء أكاديمية بالتقسيمة المعتمدة:

```text
Features/Academies/
├── Commands/
│   ├── CreateAcademyCommand.cs
│   └── CreateAcademyHandler.cs
├── Validation/CreateAcademyValidator.cs
├── Orchestrators/AcademyOrchestrator.cs
├── DTOs/CreateAcademyDto.cs
├── ViewModels/AcademyViewModel.cs
├── Queries/
│   ├── GetAcademyQuery.cs
│   └── GetAcademyHandler.cs
└── Controllers/AcademiesController.cs
```

الـ command أو query مع الـ handler الخاص به في نفس المجلد. نضيف ملفات الـ DTOs
والـ ViewModels عند الحاجة، بدون تكرار غير ضروري. المجلدات دي داخل Academies
فقط، وليست مجلدات عامة تجمع ملفات كل features المشروع.

مثال namespace:

```csharp
namespace Learning_Platform_app.Features.Academies.Commands;
```

ملفات الطلبات والاستجابات الخاصة بالقراءة توضع كذلك داخل slice العملية:
GetCoursesQuery وGetCoursesHandler وGetCoursesResponse في Features/Courses/GetCourses.
الـ endpoint يمرر الطلب ويحوّل النتيجة لاستجابة HTTP، والتحقق من صلاحيات
الأكاديمية والمستخدم يكون جزءًا واضحًا من تنفيذ العملية.

## الـ orchestrators والمعاملات

- الـ orchestrator الخاص بميزة يوضع في Features/<Feature>/Orchestrators.
  مثال: ExamOrchestrator داخل Features/Exams/Orchestrators.
- لو التنسيق يخص عملية واحدة فقط، يمكن أن يظل في الـ handler أو داخل slice نفسها.
- الـ command handler ينفذ العملية مباشرة أو يستدعي الـ orchestrator المسؤول عنها.
  قواعد العمل المشتركة توضع في مكون متخصص عند ظهور احتياج حقيقي للمشاركة.
- عقد إدارة المعاملة، مثل ITransactionExecutor، مكانه Common/Abstractions/Transactions.
  تنفيذ ExecuteInTransactionAsync وإدارة depth مكانه Infrastructure/Transactions.
- كل الـ orchestrators المتداخلة تستخدم نفس executor ونفس DbContext داخل الـ scope.
  الخارجي يدير الحفظ والـ commit، والداخلي ينضم للمعاملة. الفشل الداخلي يمنع
  الـ commit حتى لو تم التقاط الاستثناء؛ العداد وحده لا يكفي.
- الـ queries تقرأ وتعيد DTOs ولا تدخل تلقائيًا في transaction الخاصة بالـ commands.
- التسجيل كـ Scoped وحدود التنفيذ وسياسات الفشل والتعارض تحتاج تنفيذًا واختبارات؛
  تجهيز المجلدات لا يضيف هذه الوظائف تلقائيًا.

## مسؤوليات المجلدات المشتركة

| المكان | ما يوضع فيه |
|---|---|
| Domain/Entities | Academy وCourse وباقي كيانات العمل وقواعدها |
| Domain/Enums | حالات وأنواع تخص الدومين |
| Common/Abstractions/Persistence | عقود الوصول للبيانات ووحدة العمل عند استخدامها |
| Common/Abstractions/Payments | عقد بوابة الدفع الذي تحتاجه العمليات |
| Infrastructure/Persistence | DbContext وإعدادات EF والمستودعات وبيانات البداية |
| Infrastructure/Persistence/Migrations | يضاف عند إنشاء أول migration |
| Infrastructure/Payments | تكامل بوابة الدفع والتحقق من الإشعارات |
| Infrastructure/Storage | تكامل تخزين الملفات والفيديو |
| Program.cs | تركيب التطبيق وتسجيل الخدمات وربط الـ endpoints |

الأسماء الجديدة للمجلدات موحدة: Domain وInfrastructure وPersistence وConfigurations.
ملفات DependencyInjection الخاصة بالتخزين أو البوابة توضع بجانب تنفيذها داخل
Infrastructure، وتسجيل feature يوضع داخل feature عند الحاجة. لا يلزم مجلد
Injection عام.

Common يحتوي على ما تشترك فيه ميزتان فعليًا؛ الملف الخاص بعملية واحدة يظل معها.
الـ handlers لا تتعامل مع HttpContext مباشرة؛ هوية المستخدم والأكاديمية تمر عبر
عقد واضح عند تنفيذهما. لا تُعاد entities الامتحانات مباشرة للطالب لأن بعض حقولها
قد تحتوي مفاتيح الإجابات.

مجلدات الاختبارات، عند إضافة مشاريعها، تتبع نفس feature والعملية، مثل:
tests/LearningPlatform.Tests/Features/Exams/StartAttempt.

## حدود التنظيم الحالي

المجلدات مثبتة في Git بملفات .gitkeep ويمكن حذف هذه الملفات عند إضافة التنفيذ.
لم تتم إضافة مكتبة وسيط أو CQRS framework أو transaction executor ضمن ترتيب
المجلدات. الاختيار بين الاستدعاء المباشر للـ handlers ومكتبة وسيط مستقل عن
تنظيم الـ slices.

الـ namespaces داخل مشروع واحد لا تفرض عزلًا بين الوحدات أثناء البناء. الفصل
الحالي تنظيمي، ويمكن تقوية الحدود لاحقًا بمشاريع مستقلة أو اختبارات معمارية
عند وجود حاجة واضحة.
