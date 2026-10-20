# قاعدة البيانات وأول migration

المشروع يستخدم SQL Server وEF Core 10.0.12 وASP.NET Core Identity.
أداة dotnet-ef 10.0.12 مثبتة للمشروع في .config/dotnet-tools.json؛ لا يلزم
تغيير إصدار الأداة العامة الموجودة على جهاز المطوّر.

## الاتصال المحلي

- Server: `(localdb)\MSSQLLocalDB`
- Database: `LearningPlatform_Development`
- Authentication: Windows / Integrated Security.
- إعداد التطوير في appsettings.Development.json بدون كلمات مرور.
- DefaultConnection في appsettings.json فارغ؛ أي بيئة أخرى يجب أن تضبطه صراحة.

هذا الاتصال مخصص لبيئة التطوير. TrustServerCertificate مفعّل محليًا فقط.
لتغيير السيرفر أو استخدام مصادقة SQL، احفظ الاتصال في User Secrets أو متغير
البيئة ConnectionStrings__DefaultConnection، ولا تضع كلمة المرور في Git.
حساب المطوّر الحالي هو الذي يملك LocalDB، لذلك يلزم تشغيل الأوامر بنفس حساب Windows.

## أوامر التجهيز من جذر المستودع

```powershell
dotnet tool restore
dotnet restore Learning-Platform-app.slnx
dotnet build Learning-Platform-app.slnx --no-restore
dotnet ef database update --project Learning-Platform-app -- --environment Development
```

المشروع لا يطبّق migrations عند تشغيل الـ API تلقائيًا، ولا يستخدم EnsureCreated
بدل migrations. تشغيل أمر التحديث على قاعدة التطوير مرة ثانية لا يكرر الجداول.
قبل تغيير الاتصال إلى قاعدة موجودة ببيانات، راجع مخططها وتاريخ migrations وخطة
ترحيلها؛ أول migration ليست آلية لتبنّي مخطط قديم تلقائيًا.

لتوصيل SSMS أو SQL Server Object Explorer، استخدم السيرفر واسم القاعدة أعلاه
مع Windows Authentication. سيظهر سجل النسخ في جدول __EFMigrationsHistory.

## المخطط الحالي

تشمل InitialAcademySchema المخطط الأصلي، وتزيل RemoveAcademyMemberships جداول
العضويات والأدوار وفق قرار تأجيلها. لا نعدل migration سبق تطبيقها؛ القاعدة
الجديدة تشغّل التسلسل كاملًا وتصل إلى نفس المخطط الحالي.

المخطط الحالي يشمل جداول Identity القياسية، بالإضافة إلى:

- Academies: الاسم والرابط والتواصل وحالة التفعيل وتواريخ التغيير وRowVersion.
- OwnerUserId على Academies يربط مالك الأكاديمية بحساب موجود مباشرة.
- Students وInstructors: بروفايلات مرتبطة مباشرة بـ UserId وAcademyId.

حساب ApplicationUser عام، ويمكن أن يكون له بروفايل طالب أو مدرّس في أكثر من
أكاديمية. كل نوع بروفايل فريد للجهة والمستخدم، ويمكن أن يجمع المستخدم بروفايل
طالب ومدرّس في نفس الجهة. البروفايل لا يكرر حساب Identity أو بيانات دخوله.

OwnerUserId مطلوب ويشير إلى مستخدم موجود. إدارة نقل الملكية والتحقق من صلاحيات
المالك وبروفايل الطالب أو المدرّس ستنفّذ في خدمات الميزة. لا توجد حاليًا عضويات
مستقلة أو أدوار Admin داخل كل أكاديمية. لا تعني قيود العلاقات وجود authorization
أو tenant query filters جاهزة. IsActive لا يمنع الدخول تلقائيًا قبل تنفيذ الصلاحيات.

عند ترقية قاعدة قديمة، تنقل migration روابط المستخدمين من العضويات والمالك من
دور Owner قبل حذف الجدولين. إذا كانت أكاديمية قديمة بلا مالك، تتوقف الترقية
داخل معاملتها لتعيين المالك صراحة بدل تخمينه. حذف العضويات يلغي أدوارها وحالة
تفعيلها المستقلة. الرجوع يعيد إنشاء الروابط والأدوار المشتقة من المالك والبروفايلات،
ولا يستعيد أدوار Admin القديمة أو أعضاءً ليس لهم بروفايل؛ هذه بيانات مؤجلة محذوفة.

البريد المطبّع فريد عند وجوده، وUserManager مضبوط على RequireUniqueEmail.
إعداد Identity الحالي يجهّز التخزين ومزوّدي التوكنات؛ لا يضيف endpoints للدخول
أو JWT authentication. سجلات Identity Roles العامة ليست بديلًا عن صلاحيات
أكاديمية محددة؛ أدوار الأكاديمية مؤجلة حاليًا.

علاقات الدومين تستخدم NoAction للحذف للحفاظ على السجلات ومنع حذف سلسلة
علاقات بالخطأ. البيانات الحساسة أو حسابات الإدارة بكلمات مرور ثابتة لم تُزرع.
المواد والكورسات والتسجيلات والامتحانات والمبيعات ستضاف مع مهامها في migrations
تالية بدل إضافة كيانات لم يبدأ تنفيذها.

## إضافة تغييرات لاحقًا

```powershell
dotnet ef migrations add NameOfChange --project Learning-Platform-app --output-dir Infrastructure/Persistence/Migrations -- --environment Development
dotnet ef migrations script --idempotent --project Learning-Platform-app -- --environment Development
dotnet ef database update --project Learning-Platform-app -- --environment Development
```

راجع ملف migration قبل تطبيقه، خصوصًا عمليات حذف الأعمدة أو تحويل أنواعها.
لا تعدّل migration طُبّقت أو شُوركت مع الفريق؛ أضف migration جديدة للتغيير.

## الاختبارات

```powershell
dotnet test Learning-Platform-app.slnx
```

تستخدم الاختبارات SQL Server فعليًا عبر LocalDB، وتطبّق migration على قاعدة
منفصلة باسم LearningPlatform_ModelTests_ ومعرف عشوائي، ثم تحذف تلك القاعدة
وحدها بعد الانتهاء. لا تستخدم إعداد اتصال التطبيق. يمكن تحديد instance اختبار
آخر يدعم Windows Authentication عبر LEARNINGPLATFORM_TEST_SERVER.

الاختبارات تتحقق من تطبيق migration، وعدم وجود تغييرات موديل غير ممثلة فيها،
ورفض تكرار البروفايل لنفس المستخدم والجهة، وصحة المستخدم والأكاديمية المشار
إليهما، ودعم بروفايل طالب ومدرّس لنفس الحساب، وتعارض RowVersion عند تعديل قديم.
كما تتحقق من ترقية بيانات المخطط القديم والرجوع ثم الترقية مجددًا، والتوقف
بدون فقد البيانات عندما لا يمكن تحديد مالك أكاديمية قديمة.
يلزم السماح بإنشاء قاعدة اختبار وحذفها على instance الاختبار.
