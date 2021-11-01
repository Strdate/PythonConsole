from __future__ import annotations

from typing import Any, List, Optional

from .. import model
from . import gamelib


class EntityObject(model.EntityObjectABC):

    def delete(self):
        _game = gamelib.Game()
        return _game._delete_obj(self.id_, self.type)

    def move(self, pos: model.IPositionable):
        _game = gamelib.Game()
        return _game._move_obj(self.id_, self.type, pos)

    def refresh(self):
        _game = gamelib.Game()
        self.update(_game._get_obj(id_=self.id_, type_=self.type))

class RotatableEntity(model.RotatableEntityABC, EntityObject):

    def move(self, pos: model.IPositionable, angle: float):
        _game = gamelib.Game()
        return _game._move_obj(self.id_, self.type, pos, angle)


class NetworkObject(model.NetworkObjectABC, EntityObject):

    @property
    def prefab(self) -> NetPrefab:
        _game = gamelib.Game()
        return _game.get_net_prefab(self.prefab_name)

NetPrefab = model.NetPrefab

class Tree(model.TreeABC, EntityObject):
    pass


class Building(model.BuildingABC, RotatableEntity):
    pass


class Prop(model.PropABC, RotatableEntity):
    pass


class Node(model.NodeABC, NetworkObject):

    @property
    def building(self) -> Building:
        _game = gamelib.Game()
        return _game.get_building(id_=self.building_id)

    @property
    def segments(self) -> List['Segment']:
        _game = gamelib.Game()
        return _game._get_adj_segment(self.id_)


class Segment(model.SegmentABC, NetworkObject):
    
    @property
    def start_node(self) -> Node:
        _game = gamelib.Game()
        return _game.get_node(self.start_node_id)

    @property
    def end_node(self) -> Node:
        _game = gamelib.Game()
        return _game.get_node(self.end_node_id)


class NaturalResourceCell(model.NaturalResourceCellABC):

    @model.NaturalResourceCellABC.pollution.setter
    def pollution(self, pollution: int) -> None:
        _game = gamelib.Game()
        _game._set_resource(
            self.id_, 'pollution', pollution
        )

    @model.NaturalResourceCellABC.fertility.setter
    def fertility(self, fertility: int) -> None:
        _game = gamelib.Game()
        _game._set_resource(
            self.id_, 'fertility', fertility
        )
    
    @model.NaturalResourceCellABC.forest.setter
    def forest(self, forest: int) -> None:
        _game = gamelib.Game()
        _game._set_resource(
            self.id_, 'forest', forest
        )
    
    @model.NaturalResourceCellABC.ore.setter
    def ore(self, ore: int) -> None:
        _game = gamelib.Game()
        _game._set_resource(
            self.id_, 'ore', ore
        )

    @model.NaturalResourceCellABC.oil.setter
    def oil(self, oil: int) -> None:
        _game = gamelib.Game()
        _game._set_resource(
            self.id_, 'oil', oil
        )


class RenderableObjectHandle(model.RenderableObjectHandleABC):

    def delete(self):
        _game = gamelib.Game()
        return _game.remove_render_object(self.id_)

class PathBuilder(model.PathBuilderABC):

    def _create_segments(self,
        end_node: model.IPositionable,
        options: Any, start_dir: Optional[model.Vector],
        end_dir: Optional[model.Vector], middle_pos: Optional[model.Vector]
    ) -> List[Segment]:
        _game = gamelib.Game()
        ret = _game._create_segment(
            self.last_node.position, end_node, options,
            start_dir=start_dir, end_dir=end_dir, middle_pos=middle_pos,
            autosplit=True
        )
        assert ret is not None
        return ret

class NaturalResource(model.NaturalResourceCellABC):

    def fetch_data(self) -> model.BaseMessage:
        _game = gamelib.Game()
        return _game._get_resource(self.id_)

    @model.NaturalResourceCellABC.pollution.setter
    def pollution(self, pollution: int) -> None:
        _game = gamelib.Game()
        _game._set_resource(
            self.id_, 'pollution', pollution
        )

    @model.NaturalResourceCellABC.fertility.setter
    def fertility(self, fertility: int) -> None:
        _game = gamelib.Game()
        _game._set_resource(
            self.id_, 'fertility', fertility
        )
    
    @model.NaturalResourceCellABC.forest.setter
    def forest(self, forest: int) -> None:
        _game = gamelib.Game()
        _game._set_resource(
            self.id_, 'forest', forest
        )

    @model.NaturalResourceCellABC.ore.setter
    def ore(self, ore: int) -> None:
        _game = gamelib.Game()
        _game._set_resource(
            self.id_, 'ore', ore
        )
    
    @model.NaturalResourceCellABC.oil.setter
    def oil(self, oil: int) -> None:
        _game = gamelib.Game()
        _game._set_resource(
            self.id_, 'oil', oil
        )
