import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/driver_location.dart';
import '../../models/shift.dart';
import '../../providers/driver_provider.dart';
import '../../providers/shift_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/app_messages.dart';

class ReportLocationScreen extends StatefulWidget {
  const ReportLocationScreen({super.key});

  @override
  State<ReportLocationScreen> createState() => _ReportLocationScreenState();
}

class _ReportLocationScreenState extends State<ReportLocationScreen> {
  static const LatLng _fallbackCentre = LatLng(43.3438, 17.8078);
  static const Duration _autoInterval = Duration(minutes: 2);

  final _drivers = DriverProvider();
  final _shifts = ShiftProvider();
  final _mapController = MapController();

  Timer? _timer;
  Shift? _openShift;
  DriverLocation? _reported;
  LatLng? _picked;
  bool _isLoading = true;
  bool _isSending = false;
  bool _autoReport = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _timer?.cancel();
    _mapController.dispose();
    super.dispose();
  }

  bool get _isOnShift => _openShift?.isOngoing == true;

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final shift = await _shifts.readOpenShift();

      if (!mounted) return;

      setState(() {
        _openShift = shift;
        _isLoading = false;
      });

      _syncTimer();
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.message;
        _isLoading = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _error = 'Could not reach the server. Check that the API is running.';
        _isLoading = false;
      });
    }
  }

  void _syncTimer() {
    _timer?.cancel();

    if (!_autoReport || !_isOnShift) {
      return;
    }

    _timer = Timer.periodic(_autoInterval, (_) {
      final point = _picked;

      if (point != null && mounted) {
        _send(point, quiet: true);
      }
    });
  }

  Future<void> _send(LatLng point, {bool quiet = false}) async {
    if (_isSending) {
      return;
    }

    setState(() => _isSending = true);

    try {
      final reported = await _drivers.reportLocation(
        point.latitude,
        point.longitude,
      );

      if (!mounted) return;

      setState(() {
        _reported = reported;
        _isSending = false;
      });

      if (!quiet) {
        AppMessages.success(context, 'Your company can see where you are.');
      }
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() => _isSending = false);
      if (!quiet) {
        AppMessages.failure(context, e.message);
      }
    } catch (_) {
      if (!mounted) return;
      setState(() => _isSending = false);
      if (!quiet) {
        AppMessages.failure(
          context,
          'Could not reach the server. Check that the API is running.',
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Center(
        child: SizedBox(
          height: 28,
          width: 28,
          child: CircularProgressIndicator(strokeWidth: 2.5),
        ),
      );
    }

    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Text(
            _error!,
            textAlign: TextAlign.center,
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        ),
      );
    }

    return Stack(
      children: [
        _buildMap(),
        Positioned(
          top: AppSpacing.md,
          left: AppSpacing.md,
          right: AppSpacing.md,
          child: _buildHint(),
        ),
        Positioned(
          left: AppSpacing.md,
          right: AppSpacing.md,
          bottom: AppSpacing.md,
          child: _buildPanel(),
        ),
      ],
    );
  }

  Widget _buildMap() {
    final picked = _picked;
    final reported = _reported;

    final reportedPoint = reported?.latitude != null && reported?.longitude != null
        ? LatLng(reported!.latitude!, reported.longitude!)
        : null;

    return FlutterMap(
      mapController: _mapController,
      options: MapOptions(
        initialCenter: reportedPoint ?? _fallbackCentre,
        initialZoom: 14,
        minZoom: 3,
        maxZoom: 18,
        onTap: _isOnShift
            ? (_, point) => setState(() => _picked = point)
            : null,
      ),
      children: [
        TileLayer(
          urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
          userAgentPackageName: 'ba.fit.pickmeup_mobile',
          maxZoom: 19,
        ),
        MarkerLayer(
          markers: [
            if (reportedPoint != null)
              _marker(reportedPoint, AppColors.toggleOn, Icons.my_location),
            if (picked != null)
              _marker(picked, AppColors.mobileHeader, Icons.place),
          ],
        ),
      ],
    );
  }

  Marker _marker(LatLng point, Color colour, IconData icon) {
    return Marker(
      point: point,
      width: 38,
      height: 38,
      child: Container(
        decoration: BoxDecoration(
          color: colour,
          shape: BoxShape.circle,
          border: Border.all(color: Colors.white, width: 3),
        ),
        child: Icon(icon, color: Colors.white, size: 18),
      ),
    );
  }

  Widget _buildHint() {
    final theme = Theme.of(context);

    return Material(
      color: _isOnShift ? AppColors.mobileHeader : AppColors.toggleOff,
      borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      elevation: 3,
      child: Padding(
        padding: const EdgeInsets.symmetric(
          horizontal: AppSpacing.md,
          vertical: AppSpacing.sm,
        ),
        child: Row(
          children: [
            Icon(
              _isOnShift ? Icons.touch_app_outlined : Icons.bedtime_outlined,
              color: Colors.white,
              size: 20,
            ),
            const SizedBox(width: AppSpacing.sm),
            Expanded(
              child: Text(
                _isOnShift
                    ? 'Tap the map where you are, then send it.'
                    : 'You are off shift. Start a shift before reporting where you are.',
                style: theme.textTheme.bodySmall?.copyWith(color: Colors.white),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildPanel() {
    final theme = Theme.of(context);
    final reported = _reported;
    final picked = _picked;

    return Material(
      color: AppColors.surface,
      elevation: 4,
      borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      clipBehavior: Clip.antiAlias,
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text('Where you are', style: theme.textTheme.titleMedium),
            const SizedBox(height: AppSpacing.xs),
            Text(
              reported == null
                  ? 'Nothing reported yet in this session.'
                  : 'Last sent ${Formats.moment(reported.updatedAt)}.',
              style: theme.textTheme.bodySmall,
            ),
            const SizedBox(height: AppSpacing.sm),
            SwitchListTile(
              contentPadding: EdgeInsets.zero,
              value: _autoReport,
              onChanged: _isOnShift
                  ? (value) {
                      setState(() => _autoReport = value);
                      _syncTimer();
                    }
                  : null,
              title: const Text('Keep sending while on shift'),
              subtitle: const Text('Resends the pin every couple of minutes.'),
            ),
            const SizedBox(height: AppSpacing.sm),
            FilledButton.icon(
              onPressed: !_isOnShift || picked == null || _isSending
                  ? null
                  : () => _send(picked),
              icon: const Icon(Icons.send, size: 18),
              label: _isSending
                  ? const SizedBox(
                      height: 18,
                      width: 18,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: Colors.white,
                      ),
                    )
                  : Text(
                      picked == null
                          ? 'TAP THE MAP FIRST'
                          : 'SEND MY LOCATION',
                    ),
            ),
          ],
        ),
      ),
    );
  }
}
