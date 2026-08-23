import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/rating_summary.dart';
import '../../models/review.dart';
import '../../providers/driver_provider.dart';
import '../../providers/review_provider.dart';
import '../../providers/user_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/rating_stars.dart';

class DriverReviewsScreen extends StatefulWidget {
  const DriverReviewsScreen({super.key});

  @override
  State<DriverReviewsScreen> createState() => _DriverReviewsScreenState();
}

class _DriverReviewsScreenState extends State<DriverReviewsScreen> {
  final _reviews = ReviewProvider();
  final _drivers = DriverProvider();
  final _users = UserProvider();

  List<Review> _items = const [];
  RatingSummary? _summary;
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final me = await _users.readMe();
      final self = me.email == null
          ? null
          : await _drivers.findByEmail(me.email!);

      final result = await _reviews.get(
        filter: {'Page': 1, 'PageSize': 20, 'SortBy': 'CreatedAt desc'},
      );

      RatingSummary? summary;

      if (self?.id != null) {
        final averages = await _reviews.readDriverAverages();

        for (final row in averages) {
          if (row.id == self!.id) {
            summary = row;
            break;
          }
        }
      }

      if (!mounted) return;

      setState(() {
        _items = result.items ?? const <Review>[];
        _summary = summary;
        _isLoading = false;
      });
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

    final theme = Theme.of(context);

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(AppSpacing.screenPadding),
        children: [
          _buildSummary(theme),
          const SizedBox(height: AppSpacing.lg),
          if (_items.isEmpty)
            Text(
              'No passenger has reviewed you yet.',
              style: theme.textTheme.bodySmall,
            )
          else
            for (final review in _items)
              Padding(
                padding: const EdgeInsets.only(bottom: AppSpacing.sm),
                child: _ReviewTile(review: review),
              ),
        ],
      ),
    );
  }

  Widget _buildSummary(ThemeData theme) {
    final summary = _summary;

    return Container(
      padding: const EdgeInsets.all(AppSpacing.lg),
      decoration: BoxDecoration(
        color: AppColors.sectionTint,
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Column(
        children: [
          Text(
            summary?.averageRating == null
                ? '—'
                : summary!.averageRating!.toStringAsFixed(1),
            style: theme.textTheme.headlineMedium,
          ),
          const SizedBox(height: AppSpacing.xs),
          RatingStars(
            average: summary?.averageRating,
            reviewCount: summary?.reviewCount,
            showLabel: false,
          ),
          const SizedBox(height: AppSpacing.xs),
          Text(
            summary == null || (summary.reviewCount ?? 0) == 0
                ? 'No reviews yet'
                : '${summary.reviewCount} '
                      '${summary.reviewCount == 1 ? 'review' : 'reviews'} from passengers',
            style: theme.textTheme.bodySmall,
          ),
        ],
      ),
    );
  }
}

class _ReviewTile extends StatelessWidget {
  const _ReviewTile({required this.review});

  final Review review;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.surface,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  review.authorName ?? '',
                  style: theme.textTheme.titleMedium,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
              RatingStars(
                average: (review.rating ?? 0).toDouble(),
                reviewCount: 1,
                size: 14,
                showLabel: false,
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.xs),
          Text(
            'Ride #${review.orderId} · ${Formats.moment(review.createdAt)}',
            style: theme.textTheme.bodySmall,
          ),
          if ((review.comment ?? '').isNotEmpty) ...[
            const SizedBox(height: AppSpacing.xs),
            Text(review.comment!, style: theme.textTheme.bodyMedium),
          ],
        ],
      ),
    );
  }
}
