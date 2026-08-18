import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:latlong2/latlong.dart';

class GeocodedPlace {
  const GeocodedPlace({required this.label, required this.point});

  final String label;
  final LatLng point;
}

class GeocodingService {
  static const String _host = 'nominatim.openstreetmap.org';
  static const Map<String, String> _headers = {
    'User-Agent': 'ba.fit.pickmeup_mobile/1.0 (FIT RS2 seminar project)',
    'Accept': 'application/json',
  };

  Future<List<GeocodedPlace>> search(String query, {LatLng? near}) async {
    final trimmed = query.trim();

    if (trimmed.isEmpty) {
      return const [];
    }

    final parameters = <String, String>{
      'q': trimmed,
      'format': 'jsonv2',
      'limit': '6',
      'addressdetails': '0',
    };

    if (near != null) {
      parameters['viewbox'] =
          '${near.longitude - 0.4},${near.latitude + 0.3},${near.longitude + 0.4},${near.latitude - 0.3}';
      parameters['bounded'] = '0';
    }

    final response = await http.get(
      Uri.https(_host, '/search', parameters),
      headers: _headers,
    );

    if (response.statusCode >= 299) {
      return const [];
    }

    final decoded = jsonDecode(response.body);

    if (decoded is! List) {
      return const [];
    }

    final places = <GeocodedPlace>[];

    for (final entry in decoded) {
      if (entry is! Map) {
        continue;
      }

      final latitude = double.tryParse('${entry['lat']}');
      final longitude = double.tryParse('${entry['lon']}');
      final label = '${entry['display_name'] ?? ''}'.trim();

      if (latitude == null || longitude == null || label.isEmpty) {
        continue;
      }

      places.add(
        GeocodedPlace(label: label, point: LatLng(latitude, longitude)),
      );
    }

    return places;
  }

  Future<String?> describe(LatLng point) async {
    final response = await http.get(
      Uri.https(_host, '/reverse', {
        'lat': '${point.latitude}',
        'lon': '${point.longitude}',
        'format': 'jsonv2',
        'zoom': '18',
      }),
      headers: _headers,
    );

    if (response.statusCode >= 299) {
      return null;
    }

    final decoded = jsonDecode(response.body);

    if (decoded is! Map) {
      return null;
    }

    final label = '${decoded['display_name'] ?? ''}'.trim();

    return label.isEmpty ? null : label;
  }
}
