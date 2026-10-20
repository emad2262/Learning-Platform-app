# Academies

ملفات الأكاديمية مجمعة داخل الـ feature ومقسمة حسب مسؤوليتها:

| المجلد | أمثلة الملفات عند تنفيذها |
|---|---|
| Commands | CreateAcademyCommand.cs وCreateAcademyHandler.cs |
| Validation | CreateAcademyValidator.cs |
| Orchestrators | AcademyOrchestrator.cs |
| DTOs | CreateAcademyDto.cs وUpdateAcademyDto.cs لنقل بيانات الطلب |
| ViewModels | AcademyViewModel.cs للبيانات المعروضة في الاستجابة |
| Queries | GetAcademyQuery.cs وGetAcademyHandler.cs |
| Controllers | AcademiesController.cs |

كل command أو query مع الـ handler الخاص به في نفس المجلد.
الـ controller يستقبل HTTP ويستدعي الـ handler. تنسيق خطوات العملية داخل
الـ handler أو الـ orchestrator، وعقد إدارة المعاملة وتنفيذه في المكان المشترك.
نضيف DTO أو ViewModel لما يكون له دور واضح، بدون تكرار نفس البيانات في أنواع
إضافية بلا حاجة.

الـ namespace يطابق المسار، مثل:
`Learning_Platform_app.Features.Academies.Commands`.

المجلدات جاهزة لإضافة الملفات؛ لا تحتوي حاليًا على تنفيذ للعمليات.
