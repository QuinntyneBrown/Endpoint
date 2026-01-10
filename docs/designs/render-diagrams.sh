#!/bin/bash

# Script to render PlantUML and Draw.io diagrams
# Requires: Java, PlantUML jar, and optionally draw.io CLI

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PLANTUML_JAR="${PLANTUML_JAR:-plantuml.jar}"
OUTPUT_FORMAT="${OUTPUT_FORMAT:-png}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}PlantUML Microservices Designs - Diagram Renderer${NC}"
echo "=================================================="

# Check prerequisites
check_prerequisites() {
    echo -e "\n${YELLOW}Checking prerequisites...${NC}"

    if ! command -v java &> /dev/null; then
        echo -e "${RED}Error: Java is required but not installed.${NC}"
        exit 1
    fi
    echo "  Java: $(java -version 2>&1 | head -1)"

    if [ ! -f "$PLANTUML_JAR" ]; then
        echo -e "${YELLOW}PlantUML jar not found at: $PLANTUML_JAR${NC}"
        echo "  Attempting to download..."
        curl -L -o "$PLANTUML_JAR" \
            "https://github.com/plantuml/plantuml/releases/download/v1.2024.8/plantuml-1.2024.8.jar" \
            || { echo -e "${RED}Failed to download PlantUML${NC}"; exit 1; }
    fi
    echo "  PlantUML: $PLANTUML_JAR"
}

# Render PlantUML diagrams
render_plantuml() {
    echo -e "\n${YELLOW}Rendering PlantUML diagrams...${NC}"

    for design_dir in "$SCRIPT_DIR"/design-*/; do
        design_name=$(basename "$design_dir")
        echo -e "\n  Processing $design_name..."

        # Create output directory
        mkdir -p "$design_dir/images"

        # Render each .puml file
        for puml_file in "$design_dir"/*.puml; do
            if [ -f "$puml_file" ]; then
                filename=$(basename "$puml_file" .puml)
                echo "    Rendering: $filename.puml -> $filename.$OUTPUT_FORMAT"

                java -jar "$PLANTUML_JAR" \
                    -t"$OUTPUT_FORMAT" \
                    -o "images" \
                    "$puml_file" \
                    || echo -e "${RED}    Failed to render $filename${NC}"
            fi
        done
    done
}

# Render Draw.io diagrams (if draw.io CLI is available)
render_drawio() {
    echo -e "\n${YELLOW}Checking for Draw.io CLI...${NC}"

    if command -v drawio &> /dev/null; then
        echo "  Draw.io CLI found. Rendering diagrams..."

        for design_dir in "$SCRIPT_DIR"/design-*/; do
            design_name=$(basename "$design_dir")

            mkdir -p "$design_dir/images"

            for drawio_file in "$design_dir"/*.drawio; do
                if [ -f "$drawio_file" ]; then
                    filename=$(basename "$drawio_file" .drawio)
                    echo "    Rendering: $filename.drawio -> $filename.png"

                    drawio \
                        --export \
                        --format png \
                        --output "$design_dir/images/$filename.png" \
                        "$drawio_file" \
                        || echo -e "${RED}    Failed to render $filename${NC}"
                fi
            done
        done
    else
        echo -e "${YELLOW}  Draw.io CLI not found. Skipping .drawio rendering.${NC}"
        echo "  To install: https://github.com/jgraph/drawio-desktop/releases"
        echo "  Or use online: https://app.diagrams.net/"
    fi
}

# Generate summary
generate_summary() {
    echo -e "\n${GREEN}Rendering complete!${NC}"
    echo ""
    echo "Generated images:"

    for design_dir in "$SCRIPT_DIR"/design-*/; do
        design_name=$(basename "$design_dir")
        if [ -d "$design_dir/images" ]; then
            echo "  $design_name/images/"
            ls -1 "$design_dir/images/" 2>/dev/null | sed 's/^/    /'
        fi
    done
}

# Main execution
main() {
    check_prerequisites
    render_plantuml
    render_drawio
    generate_summary
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --jar)
            PLANTUML_JAR="$2"
            shift 2
            ;;
        --format)
            OUTPUT_FORMAT="$2"
            shift 2
            ;;
        --help)
            echo "Usage: $0 [options]"
            echo ""
            echo "Options:"
            echo "  --jar PATH     Path to plantuml.jar (default: plantuml.jar)"
            echo "  --format FMT   Output format: png, svg, eps (default: png)"
            echo "  --help         Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

main
