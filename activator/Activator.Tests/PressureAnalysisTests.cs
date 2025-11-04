using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Activator.Api.Data;
using Activator.Api.Services;
using Xunit;

namespace Activator.Tests
{
    public class PressureAnalysisTests
    {
        [Fact]
        public async Task AnalyzeFrame_ComputesMetrics()
        {
            var svc = new PressureAnalysisService();

            // create a CSV where a 16x16 block in the center has value 220, others are 1
            int W = 32, H = 32;
            var rows = new List<string>();
            for (int r = 0; r < H; r++)
            {
                var cols = new List<string>();
                for (int c = 0; c < W; c++)
                {
                    bool inBlock = r >= 8 && r < 24 && c >= 8 && c < 24;
                    cols.Add(inBlock ? "220" : "1");
                }
                rows.Add(string.Join(',', cols));
            }
            string csv = string.Join('\n', rows);

            var frame = new PressureFrame { Id = Guid.NewGuid(), CsvPayload = csv };
            var metric = await svc.AnalyzeFrameAsync(frame);

            Assert.True(metric.ContactAreaPercent > 0);
            Assert.Equal(220, (int)metric.PeakPressureIndex);
            Assert.True(metric.ContactAreaPercent > 10);
        }
    }
}
