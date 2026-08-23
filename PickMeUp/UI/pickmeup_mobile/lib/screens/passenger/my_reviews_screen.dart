import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/review.dart';
import '../../providers/review_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/rating_stars.dart';
import 'ride_detail_screen.dart';

class MyReviewsScreen extends StatefulWidget {
  const MyReviewsScreen({super.key});

  @override
  State<MyReviewsScreen> createState() => _MyReviewsScreenState();
}

class _MyReviewsScreenState extends State<MyReviewsScreen> {
  static const int _pageSize = 10;

  final _reviews = ReviewProvider();

  List<Review> _items = const [];
  int _page = 1;
  int _totalCount = 0;
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
      final result = await _reviews.get(
        filter: {
          'Page': _page,
          'PageSize': _pageSize,
          'SortBy': 'CreatedAt desc',
          'IncludeTotalCount': true,
        },
      );

      if (!mounted) return;

      setState(() {
        _items = result.items ?? const <Review>[];
        _totalCount = result.totalCount ?? 0;
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

  int get _pageCount =>
      _totalCount == 0 ? 1 : ((_totalCount + _pageSize - 1) ~/ _pageSize);

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Expanded(child: _buildList()),
        if (!_isLoading && _error == null && _pageCount > 1) _buildPager(),
      ],
    );
  }

  Widget _buildList() {
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

    if (_items.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Text(
            'You have not reviewed a ride yet. Open a finished ride to leave one.',
            textAlign: TextAlign.center,
            style: Theme.of(context).textTheme.bodySmall,
          ),
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        padding: const EdgeInsets.all(AppSpacing.screenPadding),
        itemCount: _items.length,
        itemBuilder: (context, index) {
          final review = _items[index];

          return Padding(
            padding: const EdgeInsets.only(bottom: AppSpacing.sm),
            child: _ReviewTile(
              review: review,
              onTap: review.orderId == null
                  ? null
                  : () async {
                      await RideDetailScreen.open(context, review.orderId!);
                      if (mounted) {
                        await _load();
                      }
                    },
            ),
          );
        },
      ),
    );
  }

  Widget _buildPager() {
    return Padding(
      padding: const EdgeInsets.fromLTRB(
        AppSpacing.screenPadding,
        0,
        AppSpacing.screenPadding,
        AppSpacing.md,
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          IconButton(
            tooltip: 'Previous page',
            icon: const Icon(Icons.chevron_left),
            onPressed: _page > 1
                ? () {
                    setState(() => _page -= 1);
                    _load();
                  }
                : null,
          ),
          Text(
            'Page $_page of $_pageCount',
            style: Theme.of(context).textTheme.bodySmall,
          ),
          IconButton(
            tooltip: 'Next page',
            icon: const Icon(Icons.chevron_right),
            onPressed: _page < _pageCount
                ? () {
                    setState(() => _page += 1);
                    _load();
                  }
                : null,
          ),
        ],
      ),
    );
  }
}

class _ReviewTile extends StatelessWidget {
  const _ReviewTile({required this.review, this.onTap});

  final Review review;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final targets = [
      if (review.companyName != null) review.companyName!,
      if (review.driverName != null) review.driverName!,
    ].join(' · ');

    return Material(
      color: AppColors.surface,
      borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
        child: Container(
          padding: const EdgeInsets.all(AppSpacing.md),
          decoration: BoxDecoration(
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
                      targets.isEmpty ? 'Your ride' : targets,
                      style: theme.textTheme.titleMedium,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  const SizedBox(width: AppSpacing.sm),
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
                Text(
                  review.comment!,
                  style: theme.textTheme.bodyMedium,
                  maxLines: 3,
                  overflow: TextOverflow.ellipsis,
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}
