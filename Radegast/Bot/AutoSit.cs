﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

using OpenMetaverse;
using OpenMetaverse.StructuredData;
                    if (!m_instance.State.IsSitting)
                    {
                        m_instance.State.SetSitting(true, Preferences.Primitive);
                        m_Timer.Enabled = true;
                    }