namespace RayTracing {
	class Program {
		static void Main(string[] args) {
			using (Window window = new Window(Settings.WINDOW_WIDTH, Settings.WINDOW_HEIGHT)) {
				window.Run();
			}
		}
	}
}
