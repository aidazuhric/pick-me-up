import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';

class MapAttribution extends StatelessWidget {
  const MapAttribution({super.key, this.bottomInset = 0});

  final double bottomInset;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.only(bottom: bottomInset),
      child: const RichAttributionWidget(
        alignment: AttributionAlignment.bottomRight,
        showFlutterMapAttribution: false,
        attributions: [TextSourceAttribution('© OpenStreetMap contributors')],
      ),
    );
  }
}
