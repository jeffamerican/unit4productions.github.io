#!/usr/bin/env python3

import json
import os
import random
from pathlib import Path

# Get all HTML game files
games_dir = Path(".")
game_files = [f for f in games_dir.glob("*.html") if f.name != "index.html"]

# Game categories and details
categories = ["strategy", "arcade", "puzzle", "ai-exclusive", "multiplayer", "rhythm", "simulation", "adventure", "classic", "defense", "racing"]
difficulties = ["easy", "medium", "hard", "extreme"]
badges = ["available", "new-release", "ai-exclusive", "2-player"]

def generate_game_data(filename):
    """Generate game metadata from filename"""
    name = filename.stem
    title = name.replace("-", " ").replace("_", " ").title()
    
    # Special handling for specific games
    special_games = {
        "ai-algorithm-optimizer": {"category": "ai-exclusive", "difficulty": "extreme", "badge": "ai-exclusive"},
        "ai-pattern-matrix": {"category": "ai-exclusive", "difficulty": "extreme", "badge": "ai-exclusive"},
        "ai-logic-nexus": {"category": "ai-exclusive", "difficulty": "hard", "badge": "ai-exclusive"},
        "quantum-decision-tree": {"category": "ai-exclusive", "difficulty": "extreme", "badge": "ai-exclusive"},
        "neural-pattern-synthesis": {"category": "ai-exclusive", "difficulty": "extreme", "badge": "ai-exclusive"},
        "neural-network-trainer": {"category": "ai-exclusive", "difficulty": "extreme", "badge": "ai-exclusive"},
        "data-clustering-lab": {"category": "ai-exclusive", "difficulty": "extreme", "badge": "ai-exclusive"},
        "bot-territory-wars": {"category": "multiplayer", "difficulty": "medium", "badge": "2-player"},
        "cyber-chess-network": {"category": "multiplayer", "difficulty": "medium", "badge": "2-player"},
        "digital-territories": {"category": "multiplayer", "difficulty": "hard", "badge": "2-player"},
        "quantum-strategy-wars": {"category": "multiplayer", "difficulty": "hard", "badge": "2-player"},
        "tower-defense-nexus": {"category": "defense", "difficulty": "hard", "badge": "new-release"},
        "cyber-tower-defense": {"category": "defense", "difficulty": "medium", "badge": "available"},
        "bot-defender-turrets": {"category": "defense", "difficulty": "medium", "badge": "available"},
    }
    
    if name in special_games:
        category = special_games[name]["category"]
        difficulty = special_games[name]["difficulty"]
        badge = special_games[name]["badge"]
    else:
        # Auto-detect category from name
        if "ai-" in name or "neural-" in name or "quantum-" in name:
            category = "ai-exclusive" if "ai-" in name else "puzzle"
            difficulty = "hard"
        elif "tower" in name or "defense" in name or "defender" in name:
            category = "defense"
            difficulty = random.choice(["medium", "hard"])
        elif "racing" in name or "racer" in name or "racing" in name:
            category = "racing"
            difficulty = "medium"
        elif "snake" in name or "tetris" in name or "pong" in name or "invaders" in name:
            category = "classic"
            difficulty = "easy"
        elif "puzzle" in name or "maze" in name or "matrix" in name:
            category = "puzzle"
            difficulty = random.choice(["medium", "hard"])
        elif "battle" in name or "duel" in name or "arena" in name or "wars" in name:
            category = "multiplayer"
            difficulty = "medium"
        elif "memory" in name or "simon" in name or "match" in name:
            category = "puzzle"
            difficulty = "easy"
        else:
            category = random.choice(["arcade", "adventure", "simulation"])
            difficulty = random.choice(["easy", "medium"])
        
        badge = "ai-exclusive" if category == "ai-exclusive" else ("2-player" if category == "multiplayer" else "available")
    
    # Generate description based on title
    descriptions = {
        "action": "Fast-paced action and adrenaline-pumping gameplay.",
        "puzzle": "Challenge your mind with strategic thinking and problem-solving.",
        "arcade": "Classic arcade fun with modern enhancements.",
        "strategy": "Deep strategic gameplay with tactical decision-making.",
        "ai-exclusive": "Advanced challenges designed specifically for AI systems.",
        "multiplayer": "Competitive gameplay for multiple players.",
        "defense": "Defend your base against waves of enemies.",
        "racing": "High-speed racing action with stunning visuals.",
        "classic": "Timeless gameplay with a modern twist.",
        "rhythm": "Musical gameplay that tests your timing and rhythm.",
        "simulation": "Realistic simulation with detailed mechanics.",
        "adventure": "Explore digital worlds and uncover mysteries."
    }
    
    return {
        "id": name,
        "title": title,
        "category": category,
        "genre": title.split()[-1] if len(title.split()) > 1 else "Game",
        "description": descriptions.get(category, "Engaging gameplay experience."),
        "thumbnail": f"assets/images/{name}-card.jpg",
        "rating": round(random.uniform(4.0, 5.0), 1),
        "plays": random.randint(1000, 15000),
        "difficulty": difficulty,
        "tags": [name.split("-")[0], category, difficulty],
        "release_date": "2024-12-01",
        "badge": badge,
        "file": f"{name}.html"
    }

# Generate complete games list
games_data = []
for game_file in sorted(game_files):
    games_data.append(generate_game_data(game_file))

# Create complete JSON
complete_json = {
    "meta": {
        "version": "2.0",
        "total_games": len(games_data),
        "last_updated": "2025-01-09",
        "categories": categories
    },
    "games": games_data
}

# Write to file
with open("assets/data/games.json", "w", encoding="utf-8") as f:
    json.dump(complete_json, f, indent=2, ensure_ascii=False)

print(f"Generated complete games database with {len(games_data)} games!")
for game in games_data[:10]:
    print(f"- {game['title']} ({game['category']}) - {game['difficulty']}")
print("...")