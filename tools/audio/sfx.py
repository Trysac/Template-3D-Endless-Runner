"""Sound effect recipes for Gauntlet Run. Each function returns a rendered Track."""

from synth import Track, note_freq, saw, square, triangle


def pulse(duty):
    return lambda p: square(p, duty)


def coin():
    """A coin: two quick bright chimes. Plays often, so it stays short."""
    t = Track(0.22)
    t.add_tone(0, 0.04, note_freq("E6"), pulse(0.25), 0.14, release=0.03)
    t.add_tone(0.045, 0.1, note_freq("B6"), pulse(0.25), 0.14, release=0.08)
    return t


def gem():
    """A gem: a glassy rising sparkle."""
    t = Track(0.7)
    for i, name in enumerate(["B5", "E6", "G#6", "B6", "E7"]):
        t.add_tone(i * 0.045, 0.08 if i < 4 else 0.35, note_freq(name), triangle, 0.28,
                   release=0.12, vibrato=0.008 if i == 4 else 0.0)
    return t


def jump():
    """A springy hop."""
    t = Track(0.22)
    t.add_tone(0, 0.12, 260, pulse(0.5), 0.14, freq_end=620, release=0.05)
    t.add_noise(0, 0.05, 0.08, smoothing=0.4, decay_power=2.0, seed=1)
    return t


def land():
    """Armoured boots hit the flagstones."""
    t = Track(0.16)
    t.add_noise(0, 0.08, 0.35, smoothing=0.75, decay_power=2.2, seed=2)
    t.add_tone(0, 0.05, 140, triangle, 0.3, freq_end=70, release=0.02)
    return t


def slide():
    """A scraping skid on stone."""
    t = Track(0.45)
    t.add_noise(0, 0.38, 0.32, smoothing=0.35, decay_power=0.9, seed=3)
    t.add_tone(0, 0.3, 520, saw, 0.04, freq_end=300, release=0.05)
    return t


def lane():
    """A short whoosh for a lane change."""
    t = Track(0.16)
    t.add_noise(0, 0.12, 0.22, smoothing=0.6, decay_power=1.2, seed=4)
    return t


def power_up():
    """A power-up: a bright upward arpeggio with a shimmer."""
    t = Track(0.8)
    for i, name in enumerate(["C5", "E5", "G5", "C6", "E6"]):
        t.add_tone(i * 0.06, 0.1 if i < 4 else 0.4, note_freq(name), pulse(0.125), 0.18,
                   release=0.1, vibrato=0.006 if i == 4 else 0.0)
    return t


def shield_break():
    """The shield takes the hit: a crash of wood and a ringing pop."""
    t = Track(0.7)
    t.add_noise(0, 0.35, 0.6, smoothing=0.55, decay_power=1.6, seed=5)
    t.add_tone(0, 0.08, 180, triangle, 0.5, freq_end=60, release=0.04)
    t.add_tone(0.02, 0.3, note_freq("A5"), triangle, 0.18, freq_end=note_freq("A4"), release=0.2)
    return t


def crash():
    """The knight crashes: a heavy clang into a falling groan of metal."""
    t = Track(1.0)
    t.add_noise(0, 0.45, 0.7, smoothing=0.7, decay_power=1.4, seed=6)
    t.add_tone(0, 0.12, 120, triangle, 0.7, freq_end=40, release=0.05)
    t.add_tone(0, 0.6, note_freq("D5") * 1.02, saw, 0.06, freq_end=note_freq("D4"), release=0.3, vibrato=0.02)
    t.add_tone(0, 0.6, note_freq("G#5"), triangle, 0.08, freq_end=note_freq("G#4"), release=0.3)
    return t


def milestone():
    """Every 500 m: a herald's call."""
    t = Track(0.9)
    for name, at, length in [("D5", 0.0, 0.09), ("D5", 0.11, 0.09), ("A5", 0.22, 0.45)]:
        t.add_tone(at, length, note_freq(name), pulse(0.25), 0.2, release=0.12)
    t.add_tone(0.22, 0.45, note_freq("D3"), triangle, 0.35, release=0.2)
    return t


def game_over():
    """The run is over: a short falling phrase."""
    t = Track(2.0)
    for name, at, length in [("A4", 0.0, 0.25), ("F4", 0.3, 0.25), ("D4", 0.6, 0.25), ("C#4", 0.9, 0.8)]:
        t.add_tone(at, length, note_freq(name), triangle, 0.35, release=0.2, vibrato=0.006)
    t.add_tone(0.9, 0.8, note_freq("A2"), triangle, 0.4, release=0.3)
    return t


def new_best():
    """A new best score: a little fanfare."""
    t = Track(1.2)
    for name, at, length in [("D5", 0.0, 0.1), ("F#5", 0.11, 0.1), ("A5", 0.22, 0.1), ("D6", 0.33, 0.55)]:
        t.add_tone(at, length, note_freq(name), pulse(0.25), 0.22, release=0.15)
    t.add_tone(0.33, 0.55, note_freq("D3"), triangle, 0.4, release=0.25)
    return t


def buy():
    """A purchase: coins pour into the shopkeeper's purse."""
    t = Track(0.5)
    for i in range(5):
        t.add_tone(i * 0.05, 0.04, note_freq("E6") * (1 + 0.04 * (i % 2)), pulse(0.25), 0.12, release=0.05)
    t.add_tone(0.25, 0.2, note_freq("A6"), triangle, 0.2, release=0.15)
    return t


def click():
    t = Track(0.08)
    t.add_tone(0, 0.025, 1300, pulse(0.5), 0.2, freq_end=900, release=0.02)
    return t


def denied():
    """A short low buzz for an action that is not allowed."""
    t = Track(0.2)
    t.add_tone(0, 0.12, 140, pulse(0.5), 0.15, release=0.04)
    return t


EFFECTS = {
    "coin.ogg": coin,
    "gem.ogg": gem,
    "jump.ogg": jump,
    "land.ogg": land,
    "slide.ogg": slide,
    "lane.ogg": lane,
    "power_up.ogg": power_up,
    "shield_break.ogg": shield_break,
    "crash.ogg": crash,
    "milestone.ogg": milestone,
    "game_over.ogg": game_over,
    "new_best.ogg": new_best,
    "buy.ogg": buy,
    "click.ogg": click,
    "denied.ogg": denied,
}
