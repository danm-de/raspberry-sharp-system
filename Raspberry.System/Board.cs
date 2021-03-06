#region References

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

#endregion

namespace Raspberry
{
    /// <summary>
    /// Represents the Raspberry Pi mainboard.
    /// </summary>
    /// <remarks>Version and revisions are based on <see cref="http://raspberryalphaomega.org.uk/2013/02/06/automatic-raspberry-pi-board-revision-detection-model-a-b1-and-b2/"/>.</remarks>
    public class Board
    {
        #region Fields

        private static readonly Lazy<Board> board = new Lazy<Board>(LoadBoard);
        private readonly Dictionary<string, string> settings;

        private const string raspberryPiProcessor = "BCM2708";
        
        #endregion

        #region Instance Management

        private Board(Dictionary<string, string> settings)
        {
            this.settings = settings;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current mainboard configuration.
        /// </summary>
        public static Board Current
        {
            get { return board.Value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a Raspberry Pi.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a Raspberry Pi; otherwise, <c>false</c>.
        /// </value>
        public bool IsRaspberryPi
        {
            get { return string.Equals(Processor, raspberryPiProcessor, StringComparison.InvariantCultureIgnoreCase); }
        }

        /// <summary>
        /// Gets the processor.
        /// </summary>
        public string Processor
        {
            get
            {
                string hardware;
                return settings.TryGetValue("Hardware", out hardware) ? hardware : null;
            }
        }

        /// <summary>
        /// Gets the board firmware version.
        /// </summary>
        public int Firmware
        {
            get
            {
                string revision;
                int firmware;
                if (settings.TryGetValue("Revision", out revision) && !string.IsNullOrEmpty(revision) && int.TryParse(revision, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out firmware))
                    return firmware;

                return 0;
            }
        }

        /// <summary>
        /// Gets the serial number.
        /// </summary>
        public string SerialNumber
        {
            get { 
                string serial;
                if (settings.TryGetValue("Serial", out serial) && !string.IsNullOrEmpty(serial))
                    return serial;

                return null;
            }
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <returns>The model name (<c>A</c> or <c>B</c>) if known; otherwise, <c>(char)0</c>.</returns>
        public char Model
        {
            get
            {
                var firmware = Firmware;
                switch(firmware)
                {
                    case 0x7:
                    case 0x8:
                    case 0x9:
                        return 'A';

                    case 0x2:
                    case 0x3:
                    case 0x4:
                    case 0x5:
                    case 0x6:
                    case 0xd:
                    case 0xe:
                    case 0xf:
                        return 'B';

                    default:
                        return (char)0;
                }
            }
        }

        /// <summary>
        /// Gets the board revision.
        /// </summary>
        /// <returns>The board revision for the given <see cref="Model"/> if known; otherwise, <c>0</c>.</returns>
        public int Revision
        {
            get
            {
                var firmware = Firmware;
                switch (firmware)
                {
                    case 0x7:
                    case 0x8:
                    case 0x9:
                        return 1;   // Model A, rev1

                    case 0x2:
                    case 0x3:
                        return 1;   // Model B, rev1

                    case 0x4:
                    case 0x5:
                    case 0x6:
                    case 0xd:
                    case 0xe:
                    case 0xf:
                        return 2;   // Model B, rev2

                    default:
                        return 0;   // Unknown
                }
            }
        }

        #endregion

        #region Private Helpers

        private static Board LoadBoard()
        {
            try
            {
                const string filePath = "/proc/cpuinfo";
                var settings = File.ReadAllLines(filePath)
                    .Where(l => !string.IsNullOrEmpty(l))
                    .Select(l =>
                    {
                        var separator = l.IndexOf(':');
                        if (separator < 0)
                            return new KeyValuePair<string, string>(l, null);
                        else
                            return new KeyValuePair<string, string>(l.Substring(0, separator).Trim(), l.Substring(separator + 1).Trim());
                    })
                    .ToDictionary(p => p.Key, p => p.Value);

                return new Board(settings);
            }
            catch
            {
                return new Board(new Dictionary<string, string>());
            }
        }

        #endregion
    }
}