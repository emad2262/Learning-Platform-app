using Learning_Platform_app.Infrastructure.Persistence;
using MediatR;
using Microsoft.Identity.Client;

namespace Learning_Platform_app.Features.Academies.Orchestrators
{
    public class AddAcademyOrchestrator(int AcademyId, int CutsomarId, string name)
        : IRequest<bool>;

    public class AddAcademyOrchestratorHandler : IRequestHandler<AddAcademyOrchestrator, bool>
    {
        private readonly IMediator _mediator;
        private readonly UnitOfWork _unitOfWork;

        public AddAcademyOrchestratorHandler(IMediator mediator,UnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Handle(AddAcademyOrchestrator request, CancellationToken cancellationToken)
        {
            await _unitOfWork.ExecuteAsync(async() =>
            {
                // add academy 
                // check academy 
                // another orchestator 
                // check name 
                return;
            });

            return true;

        }
    }
}
