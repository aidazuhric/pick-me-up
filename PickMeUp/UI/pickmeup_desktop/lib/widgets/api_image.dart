import 'package:flutter/material.dart';

import '../core/config/api_config.dart';
import '../core/theme/app_colors.dart';
import '../providers/auth_provider.dart';

class ApiImage extends StatelessWidget {
  const ApiImage.avatar({super.key, required this.url, this.radius = 16});

  final String? url;
  final double radius;

  static String? resolve(String? path) {
    if (path == null || path.trim().isEmpty) {
      return null;
    }

    final trimmed = path.trim();

    if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
      return trimmed;
    }

    final base = ApiConfig.baseUrl.endsWith('/')
        ? ApiConfig.baseUrl
        : '${ApiConfig.baseUrl}/';

    return '$base${trimmed.startsWith('/') ? trimmed.substring(1) : trimmed}';
  }

  static Map<String, String> authHeaders() {
    final token = AuthProvider.accessToken;
    return token == null ? const {} : {'Authorization': 'Bearer $token'};
  }

  @override
  Widget build(BuildContext context) {
    final resolved = resolve(url);
    final diameter = radius * 2;

    if (resolved == null) {
      return _placeholder(diameter);
    }

    return ClipOval(
      child: Image.network(
        resolved,
        headers: authHeaders(),
        width: diameter,
        height: diameter,
        fit: BoxFit.cover,
        errorBuilder: (_, _, _) => _placeholder(diameter),
        loadingBuilder: (context, child, progress) =>
            progress == null ? child : _loading(diameter),
      ),
    );
  }

  Widget _placeholder(double diameter) {
    return Container(
      width: diameter,
      height: diameter,
      decoration: const BoxDecoration(
        color: AppColors.inputFill,
        shape: BoxShape.circle,
      ),
      child: Icon(
        Icons.image_not_supported_outlined,
        size: diameter * 0.47,
        color: AppColors.textMuted,
      ),
    );
  }

  Widget _loading(double diameter) {
    return Container(
      width: diameter,
      height: diameter,
      decoration: const BoxDecoration(
        color: AppColors.inputFill,
        shape: BoxShape.circle,
      ),
    );
  }
}
