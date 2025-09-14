part of 'cards_bloc.dart';

abstract class CardsEvent extends Equatable {
  const CardsEvent();
  @override
  List<Object?> get props => [];
}

class CardsRequested extends CardsEvent {
  const CardsRequested();
}

class CardCreateRequested extends CardsEvent {
  const CardCreateRequested(this.request, this.completer);
  final CreateCardRequest request;
  final Completer<CardResponse?> completer;
}

class CardAssignRequested extends CardsEvent {
  const CardAssignRequested(this.id, this.userId, this.completer);
  final String id;
  final String userId;
  final Completer<void> completer;
}

class CardTerminationRequested extends CardsEvent {
  const CardTerminationRequested(this.id, this.completer);
  final String id;
  final Completer<void> completer;
}

class CardPrintRequested extends CardsEvent {
  const CardPrintRequested(this.id, this.completer);
  final String id;
  final Completer<void> completer;
}

class CardUpdateRequested extends CardsEvent {
  const CardUpdateRequested(this.id, this.request, this.completer);
  final String id;
  final UpdateCardRequest request;
  final Completer<CardResponse?> completer;
}

class UsersLoadRequested extends CardsEvent {
  const UsersLoadRequested();
}
