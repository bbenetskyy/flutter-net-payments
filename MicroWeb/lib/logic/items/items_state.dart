part of 'items_bloc.dart';

class ItemsState extends Equatable {
  const ItemsState._({required this.items, required this.loading, this.error});
  const ItemsState.initial() : this._(items: const [], loading: false);
  final List<Item> items;
  final bool loading;
  final String? error;

  ItemsState copyWith({List<Item>? items, bool? loading, String? error}) =>
      ItemsState._(items: items ?? this.items, loading: loading ?? this.loading, error: error);

  @override
  List<Object?> get props => [items, loading, error];
}
