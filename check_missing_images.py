#!/usr/bin/env python3
"""
Check for missing game card images by comparing HTML references with actual files.
"""

import os
import re
import glob
from pathlib import Path

def extract_image_references(html_file):
    """Extract all image references from HTML file."""
    with open(html_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Find all image references in assets/images/*-card.jpg
    pattern = r'assets/images/([^"]+?-card\.jpg)'
    matches = re.findall(pattern, content)
    
    # Remove duplicates and sort
    unique_matches = sorted(set(matches))
    return unique_matches

def get_existing_images(images_dir):
    """Get list of existing image files."""
    jpg_files = glob.glob(os.path.join(images_dir, "*-card.jpg"))
    # Extract just the filename without path
    filenames = [os.path.basename(f) for f in jpg_files]
    return sorted(filenames)

def main():
    # Paths
    html_file = "index.html"
    images_dir = "assets/images"
    
    print("Bot Liberation Games - Missing Images Check")
    print("=" * 50)
    
    # Extract references from HTML
    print("Extracting image references from HTML...")
    referenced_images = extract_image_references(html_file)
    print(f"Found {len(referenced_images)} unique image references")
    
    # Get existing images
    print("Scanning existing images...")
    existing_images = get_existing_images(images_dir)
    print(f"Found {len(existing_images)} existing image files")
    
    # Find missing images
    print("\nChecking for missing images...")
    missing_images = []
    existing_set = set(existing_images)
    
    for ref_img in referenced_images:
        if ref_img not in existing_set:
            missing_images.append(ref_img)
    
    # Report results
    if missing_images:
        print(f"\nMISSING IMAGES ({len(missing_images)}):")
        print("-" * 30)
        for i, img in enumerate(missing_images, 1):
            print(f"{i:2d}. {img}")
        
        print(f"\nSUMMARY:")
        print(f"Referenced: {len(referenced_images)}")
        print(f"Existing:   {len(existing_images)}")
        print(f"Missing:    {len(missing_images)}")
        print(f"Coverage:   {((len(referenced_images) - len(missing_images)) / len(referenced_images) * 100):.1f}%")
    else:
        print("\nAll referenced images exist!")
    
    # Check for unused images
    print(f"\nChecking for unused images...")
    referenced_set = set(referenced_images)
    unused_images = [img for img in existing_images if img not in referenced_set]
    
    if unused_images:
        print(f"\nUNUSED IMAGES ({len(unused_images)}):")
        print("-" * 30)
        for i, img in enumerate(unused_images, 1):
            print(f"{i:2d}. {img}")

if __name__ == "__main__":
    main()