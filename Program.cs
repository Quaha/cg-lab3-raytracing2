using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing {
	class Program {
		static void Main(string[] args) {
			using (Window window = new Window(Settings.WINDOW_WIDTH, Settings.WINDOW_HEIGHT)) {
				window.Run();
			}
		}
	}
}
