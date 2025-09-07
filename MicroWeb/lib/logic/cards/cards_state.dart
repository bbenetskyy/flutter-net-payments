part of 'cards_bloc.dart';

class CardsState extends Equatable {
  const CardsState({this.loading = false, this.items = const <CardResponse>[], this.error});

  const CardsState.initial() : this();

  final bool loading;
  final List<CardResponse> items;
  final String? error;

  CardsState copyWith({bool? loading, List<CardResponse>? items, String? error}) {
    return CardsState(loading: loading ?? this.loading, items: items ?? this.items, error: error);
  }

  @override
  List<Object?> get props => [loading, items, error];
}
