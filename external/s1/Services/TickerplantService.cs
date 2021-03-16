using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Tp
{
  public class TickerPlantService : TickerPlant.TickerPlantBase
  {
    private readonly ILogger<TickerPlantService> _logger;
    public TickerPlantService(ILogger<TickerPlantService> logger)
    {
      _logger = logger;
    }

    public override async Task Subscribe(SubscriptionRequest request, IServerStreamWriter<TickMessage> responseStream, ServerCallContext context)
    {
      var r = 0.01;
      var v = 0.2;
      var s0 = 100.0;

      double evolve(double s, double dt, double dz) {
        var alpha = (r - 0.5 * v * v) * dt + dz * Math.Sqrt(dt);
        return s * Math.Exp(alpha);
      }

      IEnumerable<double> simulate(double t, double T, double steps) {
        var dt = (T - t) / steps;
        var mt = new MathNet.Numerics.Random.MersenneTwister();
        var st = s0;
        var ct = t;
        while (ct <= T)
        {
          var dz = MathNet.Numerics.Distributions.Normal.Sample(mt, 0.0, 1.0);
          st = evolve(st, dt, dz);
          ct += dt;
          yield return st;
        }
      }

      foreach (var d in simulate(0.0, 1.0, 252.0))
      {
        var rs = new TickMessage {Value = d};
        await responseStream.WriteAsync(rs);
        Thread.Sleep(50);
      }
    }
  }
}
