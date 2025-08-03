using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Domain.Enums
{
    public enum MissionStatus
    {
        Requested,
        Approved,
        Planned,
        InProgress,
        Completed,
        Rejected
    }
}
