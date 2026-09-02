using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecruitmentTests.Models;

namespace RecruitmentTests.Services
{
    public class RecruitmentService : IRecruitmentService
    {
        public bool ValidateCandidate(Candidate candidate)
        {
            return candidate.ExperienceYears > 0;
        }

        public Candidate GetCandidateById(int id)
        {
            return new Candidate { Id = id, Name = "Test Candidate", ExperienceYears = 3 };
        }
    }

}
