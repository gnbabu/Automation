using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecruitmentTests.Models;

namespace RecruitmentTests.Services
{
    public interface IRecruitmentService
    {
        bool ValidateCandidate(Candidate candidate);
        Candidate GetCandidateById(int id);
    }

}
