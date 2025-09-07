part of 'cards_bloc.dart';

abstract class CardsEvent extends Equatable {
  const CardsEvent();
  @override
  List<Object?> get props => [];
}

class CardsRequested extends CardsEvent {
  const CardsRequested();
}
