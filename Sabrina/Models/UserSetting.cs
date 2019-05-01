using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class UserSetting
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public int SettingId { get; set; }
        public string Value { get; set; }

        public virtual Users User { get; set; }
    }
}
