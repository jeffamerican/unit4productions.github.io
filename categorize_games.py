#!/usr/bin/env python3
"""
Game Categorization Script
Intelligently assigns multiple categories to games based on titles and game types
"""

import json
import re

def get_game_categories(title, current_category, genre, description=""):
    """Determine appropriate categories for a game based on its title and properties"""
    title_lower = title.lower()
    categories = set()
    
    # Always include the current category as a starting point
    if current_category:
        categories.add(current_category)
    
    # Strategy games (2-player board games)
    if any(word in title_lower for word in ['chess', 'checkers', 'go ', 'backgammon']):
        categories.update(['strategy', '2-player', 'classic'])
    
    # Connect Four variants
    if 'connect four' in title_lower or 'connect-four' in title_lower:
        categories.update(['puzzle', '2-player', 'strategy'])
        categories.discard('simulation')  # Remove incorrect simulation category
    
    # Tic-tac-toe variants
    if 'tic' in title_lower and 'tac' in title_lower and 'toe' in title_lower:
        categories.update(['puzzle', '2-player', 'classic'])
        categories.discard('simulation')  # Remove incorrect simulation category
    
    # Racing games
    if any(word in title_lower for word in ['racing', 'race', 'circuit', 'speed', 'drift']):
        categories.update(['racing', 'arcade'])
    
    # Puzzle games
    if any(word in title_lower for word in ['puzzle', 'match', 'tetris', 'blocks', 'crystal', 'gem', 'swap']):
        categories.add('puzzle')
    
    # Card games
    if any(word in title_lower for word in ['poker', 'cards', 'solitaire', 'blackjack']):
        categories.update(['classic', '2-player'])
    
    # Arcade games
    if any(word in title_lower for word in ['blaster', 'asteroid', 'space', 'shooter', 'invader', 'breakout', 'pong']):
        categories.update(['arcade', 'classic'])
    
    # Tower Defense
    if any(word in title_lower for word in ['tower', 'defense', 'defend', 'turret']):
        categories.update(['defense', 'strategy'])
    
    # AI-specific games
    if any(word in title_lower for word in ['ai ', 'algorithm', 'neural', 'matrix', 'nexus', 'quantum']):
        if 'ai-exclusive' in categories:
            categories.update(['ai-exclusive', 'strategy'])
        else:
            # AI-themed but not exclusive
            categories.add('strategy')
    
    # Multiplayer indicators
    if any(word in title_lower for word in ['network', 'online', 'multi', 'battle', 'versus', 'pvp']):
        categories.add('multiplayer')
        if 'multi' in title_lower:
            categories.add('4-player')
    
    # 2-player indicators (but not multiplayer lobby games)
    if any(word in title_lower for word in ['vs ', 'versus', 'battle', 'duel']) and 'network' not in title_lower:
        categories.add('2-player')
    
    # Adventure/RPG games
    if any(word in title_lower for word in ['quest', 'adventure', 'exploration', 'journey']):
        categories.add('adventure')
    
    # Simulation games
    if any(word in title_lower for word in ['simulator', 'tycoon', 'management', 'builder']):
        categories.add('simulation')
    
    # Classic games
    if any(word in title_lower for word in ['classic', 'retro', 'vintage', 'old school']):
        categories.add('classic')
    
    # Remove contradictions
    if '2-player' in categories and 'multiplayer' in categories:
        # If it's specifically 2-player, remove generic multiplayer
        if any(word in title_lower for word in ['chess', 'checkers', 'connect four', 'tic tac toe']):
            categories.discard('multiplayer')
    
    # Ensure at least one category
    if not categories:
        categories.add('arcade')  # Default fallback
    
    return sorted(list(categories))

def update_games_with_categories():
    """Update the games.json file with proper multiple categories"""
    
    # Load current data
    with open('assets/data/games.json', 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    # Update categories metadata
    all_categories = [
        "2-player", "4-player", "ai-exclusive", "adventure", "arcade", 
        "classic", "defense", "multiplayer", "puzzle", "racing", 
        "simulation", "strategy"
    ]
    data['meta']['categories'] = all_categories
    
    print(f"Processing {len(data['games'])} games...")
    
    # Update each game
    for game in data['games']:
        old_category = game.get('category', 'arcade')
        new_categories = get_game_categories(
            game['title'], 
            old_category, 
            game.get('genre', ''),
            game.get('description', '')
        )
        
        # Replace single category with categories array
        game['categories'] = new_categories
        # Keep the original category field for backward compatibility initially
        game['category'] = new_categories[0]  # Primary category
        
        print(f"{game['title']:<30} | {old_category:<12} -> {', '.join(new_categories)}")
    
    # Save updated data
    with open('assets/data/games.json', 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)
    
    print(f"\n✅ Updated {len(data['games'])} games with multiple categories!")
    
    # Print new category distribution
    category_counts = {}
    for game in data['games']:
        for category in game['categories']:
            category_counts[category] = category_counts.get(category, 0) + 1
    
    print("\nNew category distribution:")
    for category in sorted(category_counts.keys()):
        print(f"  {category}: {category_counts[category]} games")

if __name__ == "__main__":
    update_games_with_categories()