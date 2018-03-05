using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot
{

    /// <summary>
    /// Interface to mark objects as killables.
    /// </summary>
    interface Killable
    {

        /// <summary>
        /// Saves data.
        /// </summary>
        void Kill();

    }
}
