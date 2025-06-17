window.BlazorToastJsServices = {
  getSystemTheme: () => {
    return window.matchMedia('(prefers-color-scheme: dark)').matches
      ? 'dark'
      : 'light';
    },
    playSound: (url) => {
        try {
            const audio = new Audio(url);
            audio.play().catch(e => console.warn("Toast sound playback failed:", e));
        } catch (e) {
            console.error("Error playing toast sound:", e);
        }
    }
};
