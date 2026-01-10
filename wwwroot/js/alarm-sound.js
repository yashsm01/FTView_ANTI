class AlarmSoundManager {
    constructor() {
        this.ctx = null;
        this.oscillator = null;
        this.gainNode = null;
        this.isPlaying = false;
        this.isInitialized = false;
        this.shouldBePlaying = false; // New flag to track if we want to play

        // Auto-initialize on first user interaction to satisfy browser policy
        const init = () => {
            if (!this.isInitialized) {
                this.initContext();
                if (this.shouldBePlaying) {
                    this.start();
                }
                window.removeEventListener('click', init);
                window.removeEventListener('keydown', init);
                window.removeEventListener('touchstart', init);
            }
        };
        window.addEventListener('click', init);
        window.addEventListener('keydown', init);
        window.addEventListener('touchstart', init);
    }

    initContext() {
        try {
            this.ctx = new (window.AudioContext || window.webkitAudioContext)();
            this.isInitialized = true;
            console.log("Audio Context Initialized");
        } catch (e) {
            console.error("Web Audio API not supported", e);
        }
    }

    start() {
        this.shouldBePlaying = true;
        if (!this.isInitialized || this.isPlaying) return;

        if (this.ctx.state === 'suspended') {
            this.ctx.resume();
        }

        this.isPlaying = true;
        this.playPulse();
    }

    stop() {
        this.shouldBePlaying = false;
        this.isPlaying = false;
        if (this.oscillator) {
            try {
                this.oscillator.stop();
                this.oscillator.disconnect();
            } catch (e) { }
            this.oscillator = null;
        }
    }

    playPulse() {
        if (!this.isPlaying) return;

        this.oscillator = this.ctx.createOscillator();
        this.gainNode = this.ctx.createGain();

        this.oscillator.type = 'sine';
        this.oscillator.frequency.setValueAtTime(880, this.ctx.currentTime); // A5 note

        this.gainNode.gain.setValueAtTime(0, this.ctx.currentTime);
        this.gainNode.gain.linearRampToValueAtTime(0.1, this.ctx.currentTime + 0.1);
        this.gainNode.gain.linearRampToValueAtTime(0, this.ctx.currentTime + 0.5);

        this.oscillator.connect(this.gainNode);
        this.gainNode.connect(this.ctx.destination);

        this.oscillator.start();
        this.oscillator.stop(this.ctx.currentTime + 0.5);

        this.oscillator.onended = () => {
            if (this.isPlaying) {
                setTimeout(() => this.playPulse(), 1000); // Beep every 1.5 seconds
            }
        };
    }
}

const alarmSound = new AlarmSoundManager();
window.alarmSound = alarmSound;
