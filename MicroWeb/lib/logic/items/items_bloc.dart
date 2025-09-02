import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../data/models/item.dart';
import '../../data/repositories/item_repository.dart';

part 'items_event.dart';
part 'items_state.dart';

class ItemsBloc extends Bloc<ItemsEvent, ItemsState> {
  ItemsBloc(this._repo) : super(const ItemsState.initial()) {
    on<ItemsRequested>(_onRequested);
    on<ItemRefreshed>(_onRefreshed);
    on<ItemSaved>(_onSaved);
    on<ItemDeleted>(_onDeleted);
  }

  final ItemRepository _repo;

  Future<void> _onRequested(ItemsRequested event, Emitter<ItemsState> emit) async {
    emit(state.copyWith(loading: true));
    try {
      final data = await _repo.fetchItems();
      emit(state.copyWith(loading: false, items: data));
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
    }
  }

  Future<void> _onRefreshed(ItemRefreshed event, Emitter<ItemsState> emit) async {
    try {
      final item = await _repo.fetchItem(event.id);
      final updated = [...state.items];
      final idx = updated.indexWhere((e) => e.id == event.id);
      if (idx != -1) {
        updated[idx] = item;
        emit(state.copyWith(items: updated));
      }
    } catch (e) {
      // ignore errors for refresh in demo
    }
  }

  Future<void> _onSaved(ItemSaved event, Emitter<ItemsState> emit) async {
    final saved = await _repo.upsertItem(event.item);
    final updated = [...state.items];
    final idx = updated.indexWhere((e) => e.id == saved.id);
    if (idx == -1) {
      updated.insert(0, saved);
    } else {
      updated[idx] = saved;
    }
    emit(state.copyWith(items: updated));
  }

  Future<void> _onDeleted(ItemDeleted event, Emitter<ItemsState> emit) async {
    await _repo.deleteItem(event.id);
    emit(state.copyWith(items: state.items.where((e) => e.id != event.id).toList()));
  }
}
