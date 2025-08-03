using gestionMissionBack.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace gestionMissionBack.Application.DTOs.TaskMission
{
    public class TaskMissionDtoGet
    {
        public int TaskId { get; set; }

        public string Description { get; set; }

        public DateTime AssignmentDate { get; set; }

        public DateTime? CompletionDate { get; set; }

        public TasksStatus Status { get; set; }

        public int MissionId { get; set; }

        public int SiteId { get; set; }

        public string SiteName { get; set; }

        public bool IsFirstTask { get; set; }
    }
}
