part of 'items_bloc.dart';

abstract class ItemsEvent extends Equatable {
  const ItemsEvent();
  @override
  List<Object?> get props => [];
}

class ItemsRequested extends ItemsEvent {}

class ItemRefreshed extends ItemsEvent {
  const ItemRefreshed(this.id);
  final String id;
}

class ItemSaved extends ItemsEvent {
  const ItemSaved(this.item);
  final Item item;
}

class ItemDeleted extends ItemsEvent {
  const ItemDeleted(this.id);
  final String id;
}
