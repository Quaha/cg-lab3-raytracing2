namespace RayTracing {
    class FrameCounter {

        int frame_сount = 0;
        float time_elapsed = 0f;

        public void updateCounter(float time) {
            time_elapsed += time;
            frame_сount++;
        }

        public bool canGetFPS() {
            return time_elapsed >= 1.0f;
        }

        public int getFPS() {
            time_elapsed = 0f;

            int result = frame_сount;
            frame_сount = 0;

            return result;
        }
    }
}